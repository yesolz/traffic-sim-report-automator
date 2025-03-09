import os
from fastapi import FastAPI, Request
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse, JSONResponse
from fastapi.templating import Jinja2Templates
from .service import jsonconverter as jsc
import time
import traceback  # 추가

app = FastAPI(docs_url=None, redoc_url=None)
templates = Jinja2Templates(directory="templates")

# 📌 환경 변수에서 base_name 가져오기
base_name = os.getenv("BASE_NAME")

if not base_name:  # None 또는 빈 문자열일 경우 예외 발생
    print("❌ BASE_NAME 환경 변수가 설정되지 않았습니다.")
    raise ValueError("🚨 BASE_NAME 환경 변수가 없습니다. watchdog_runner.py에서 제대로 전달되었는지 확인하세요.")

# 📌 `mappingDesciption` 가져오기 (ImportError 예외 처리)
try:
    from mock.mappingDescription import mappingDesciption
except ImportError:
    print("⚠️ Warning: mappingDesciption을 불러올 수 없습니다. 기본값으로 설정합니다.")
    mappingDesciption = {}

# 📌 `mappingTitle` 가져오기 (ImportError 예외 처리)
try:
    from mock.mappingTitle import mappingTitle
except ImportError:
    print("⚠️ Warning: mappingTitle을 불러올 수 없습니다. 기본값으로 설정합니다.")
    mappingTitle = {}

@app.get("/", response_class=HTMLResponse)
async def result(request: Request):
    try:
        start = time.time()
        data = jsc.json_converter(f"{base_name}.xlsx", f"{base_name}_Raw.xlsx")
        # Save analysis result to output.json
        data.save_to_json(output_dir="outputs")

        result_html = templates.TemplateResponse("index.html", {
            "request": request, 
            "data": { 
                "simulationSettings": data.get_simulationSetting() or {},
                "realTimeData": data.get_realtimeMetrics() or {},
                "mappingDesciption": mappingDesciption,  # 기본값 있음
                "mappingTitle": mappingTitle,  # 기본값 있음
                "accidentRiskData": data.get_accidentRiskRateMetrics() or {},
                "otherData": data.get_otherMetrics() or {},
                "chartData": data.get_chartData() or {}
            }
        })

        return result_html
    except Exception as e:
        print("🔥 서버에서 예외 발생! 🔥")
        print(traceback.format_exc())  # 전체 에러 로그 출력
        return JSONResponse(
            status_code=500,
            content={"message": "서버 내부 오류 발생", "detail": str(e)},
        )

app.mount("/", StaticFiles(directory="public"), name="public")

# 📌 전역 예외 핸들러 추가 (디버깅용)
@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    print("🔥 서버에서 예외 발생! 🔥")
    print(traceback.format_exc())  # 전체 에러 로그 출력
    return JSONResponse(
        status_code=500,
        content={"message": "서버 내부 오류 발생", "detail": str(exc)},
    )
