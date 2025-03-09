from fastapi import FastAPI, Request
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse, FileResponse
from fastapi.templating import Jinja2Templates
#from mock.simulationSettings import simulationSettings
from mock.mappingDescription import mappingDesciption
from mock.mappingTitle import mappingTitle
# from mock.tableRealTimeData import realTimeData
# from mock.tableLegalCompliance import legalCompliance
# from mock.tableAccidentRiskData import accidentRiskData
# from mock.tableOtherData import otherData
# from mock.chartData import chartData
import time
import json
from .service import jsonconverter as jsc

app = FastAPI(docs_url=None, redoc_url=None)
templates = Jinja2Templates(directory="templates")
filename = ["temp_010_LosE"]

@app.get("/", response_class=HTMLResponse)
async def result(request: Request):
    start = time.time()
    for i in filename:
        data=jsc.json_converter(f"{i}.xlsx",f"{i}_Raw.xlsx")
        # Save analysis result to output.json
        data.save_to_json(output_dir="outputs")
        result_html=templates.TemplateResponse("index.html", {
                "request": request, 
                "data": { 
                    "simulationSettings": data.get_simulationSetting(),#simulationSettings,
                    "realTimeData": data.get_realtimeMetrics(),#realTimeData, 
                    "mappingDesciption": mappingDesciption,
                    "mappingTitle": mappingTitle,
                    #"legalCompliance": data.get_legalComplianceMetrics(),#legalComplince,
                    "accidentRiskData": data.get_accidentRiskRateMetrics(),#accidentRiskData,
                    "otherData": data.get_otherMetrics(),#otherData,
                    "chartData": data.get_chartData()#chartData
                }
            })
    #print((time.time()-start)/4)
    return result_html
app.mount("/", StaticFiles(directory="public"), name="public")
