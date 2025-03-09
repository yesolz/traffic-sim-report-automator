import time
import subprocess
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import os

# 📌 감시할 디렉토리 설정 (Excel 파일 저장 위치)
WATCH_DIRECTORY = os.path.join(os.path.dirname(__file__), "server", "data")

class ExcelFileHandler(FileSystemEventHandler):
    def on_created(self, event):
        if event.is_directory:
            return
        if event.src_path.endswith(".xlsx"):  # 엑셀 파일 감지
            print(f"새로운 결과 파일 감지: {event.src_path}")
            self.run_python_server()

    def run_python_server(self):
        print("🔄 Python 리포팅 서버 실행 중...")
        subprocess.run(["poetry", "run", "uvicorn", "server.main:app", "--reload"])

if __name__ == "__main__":
    event_handler = ExcelFileHandler()
    observer = Observer()
    observer.schedule(event_handler, WATCH_DIRECTORY, recursive=False)
    
    print(f"👀 감시 시작: {WATCH_DIRECTORY}")
    observer.start()
    
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    
    observer.join()
