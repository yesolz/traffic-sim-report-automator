import time
import subprocess
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import os

# 📌 감시할 디렉토리 설정 (Excel 파일 저장 위치)
WATCH_DIRECTORY = os.path.join(os.path.dirname(__file__), "server", "data")

# 감지된 파일을 저장할 딕셔너리
detected_files = {}

class ExcelFileHandler(FileSystemEventHandler):
    def on_created(self, event):
        if event.is_directory:
            return

        file_path = event.src_path
        file_name = os.path.basename(file_path)

        # `_Raw.xlsx` 또는 `.xlsx` 파일인지 확인
        if file_name.endswith(".xlsx"):
            base_name = file_name.replace("_Raw.xlsx", "").replace(".xlsx", "")

            # 파일 감지 기록
            if base_name not in detected_files:
                detected_files[base_name] = set()

            detected_files[base_name].add(file_name)

            # `_Raw.xlsx`와 `.xlsx` 두 개의 파일이 존재하면 실행
            if f"{base_name}.xlsx" in detected_files[base_name] and f"{base_name}_Raw.xlsx" in detected_files[base_name]:
                print(f"✅ {base_name}.xlsx & {base_name}_Raw.xlsx 감지 완료! 서버 실행")
                self.run_python_server()
                # 실행 후 해당 파일 세트 제거 (다시 감지 가능하도록)
                del detected_files[base_name]

    def run_python_server(self):
        print("🔄 Python 리포팅 서버 실행 중...")
        process = subprocess.run(
            ["poetry", "run", "uvicorn", "server.main:app", "--reload"],
            stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True
        )
        print(f"📝 서버 로그 출력:\n{process.stdout}")
        print(f"⚠️ 서버 오류 로그:\n{process.stderr}")


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
