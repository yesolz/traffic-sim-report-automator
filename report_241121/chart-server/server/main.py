import os
from fastapi import FastAPI, Request
from fastapi.responses import FileResponse, JSONResponse, HTMLResponse
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from .service import jsonconverter as jsc
import time
import traceback

app = FastAPI(docs_url=None, redoc_url=None)
templates = Jinja2Templates(directory="templates")

# 📌 다운로드할 파일이 있는 디렉토리 설정
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")

# 📌 환경 변수에서 base_name 가져오기
base_name = os.getenv("BASE_NAME")

if not base_name:
    print("❌ BASE_NAME 환경 변수가 설정되지 않았습니다.")
    raise ValueError("🚨 BASE_NAME 환경 변수가 없습니다. watchdog_runner.py에서 제대로 전달되었는지 확인하세요.")

# 📌 `.fzp`, `.xlsx`, `_Raw.xlsx` 파일 목록 가져오기
try:
    available_files = os.listdir(DATA_DIR) if os.path.exists(DATA_DIR) else []
    print(f"📂 존재하는 파일 목록: {available_files}")  # 디버깅 로그 추가
except Exception as e:
    print("🔥 파일 목록을 가져오는 중 오류 발생!")
    print(traceback.format_exc())
    available_files = []
    
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
        data.save_to_json(output_dir="outputs")

        # 📌 다운로드할 파일 리스트 (파일 존재 여부 체크 후 안전하게 처리)
        fzp_file = next((f for f in available_files if f.endswith(".fzp")), None)
        xlsx_file = f"{base_name}.xlsx" if f"{base_name}.xlsx" in available_files else None
        raw_xlsx_file = f"{base_name}_Raw.xlsx" if f"{base_name}_Raw.xlsx" in available_files else None

        # 파일이 없을 경우 로그 출력
        if not xlsx_file or not raw_xlsx_file:
            print(f"⚠️ 다운로드할 xlsx 파일을 찾을 수 없습니다. base_name: {base_name}")

        result_html = templates.TemplateResponse("index.html", {
            "request": request,
            "data": {
                "simulationSettings": data.get_simulationSetting() or {},
                "realTimeData": data.get_realtimeMetrics() or {},
                "mappingDesciption": mappingDesciption,  # 기본값 있음
                "mappingTitle": mappingTitle,  # 기본값 있음
                "accidentRiskData": data.get_accidentRiskRateMetrics() or {},
                "otherData": data.get_otherMetrics() or {},
                "chartData": data.get_chartData() or {},
            },
            "download_files": {
                "fzp": fzp_file if fzp_file else "",
                "xlsx": xlsx_file if xlsx_file else "",
                "raw_xlsx": raw_xlsx_file if raw_xlsx_file else "",
            },
            "base_name": base_name
        })
        return result_html
    except Exception as e:
        print("🔥 서버에서 예외 발생! 🔥")
        print(traceback.format_exc())
        return JSONResponse(
            status_code=500,
            content={"message": "서버 내부 오류 발생", "detail": str(e)},
        )

# 📌 파일 다운로드 API
@app.get("/download/{file_name}")
async def download_file(file_name: str):
    file_path = os.path.join(DATA_DIR, file_name)

    if os.path.exists(file_path):
        return FileResponse(path=file_path, filename=file_name, media_type="application/octet-stream")
    else:
        return JSONResponse(status_code=404, content={"message": f"파일 '{file_name}'을 찾을 수 없습니다."})

app.mount("/static", StaticFiles(directory="public"), name="public")
