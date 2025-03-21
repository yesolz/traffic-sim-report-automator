import time
import subprocess
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import os
import sys
import re

# 📌 감시할 디렉토리 설정 (Excel 파일 저장 위치)
WATCH_DIRECTORY = os.path.join(os.path.dirname(__file__), "server", "data")

# 감지된 파일을 저장할 딕셔너리
detected_files = {}

# 🌎 유저가 접속할 서버 URL (포트는 변경 가능)
HOST = "127.0.0.1"
PORT = 8000  # 필요하면 다른 포트로 변경 가능

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
                self.run_python_server(base_name)
                # 실행 후 해당 파일 세트 제거 (다시 감지 가능하도록)
                del detected_files[base_name]

    def run_python_server(self, base_name):
        
        # base_name에서 fzp_base_name 추출
        fzp_base_name = base_name
        
        print(f"🔄 Python 리포팅 서버 실행 중... (파일: {fzp_base_name}.fzp, {base_name}.xlsx, {base_name}_Raw.xlsx)")

        # 환경 변수에 BASE_NAME, FZP_BASE_NAME 저장
        env = os.environ.copy()
        env["BASE_NAME"] = base_name
        env["FZP_BASE_NAME"] = fzp_base_name 

        # 서버 URL 출력
        server_url = f"http://{HOST}:{PORT}"
        print(f"🌎 브라우저에서 접근 가능: {server_url}")

        # `uvicorn` 실행
        process = subprocess.Popen(
            ["poetry", "run", "uvicorn", "server.main:app", "--reload", "--host", HOST, "--port", str(PORT)],
            env=env,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            bufsize=1  # 한 줄씩 출력
        )

        # stdout & stderr을 실시간으로 읽어서 출력
        while True:
            output = process.stdout.readline()
            if output:
                print(f"📝 서버 로그: {output.strip()}")
                sys.stdout.flush()  # 즉시 출력
            
            error = process.stderr.readline()
            if error:
                if "ERROR" in error or "CRITICAL" in error:
                    print(f"⚠️ 서버 오류: {error.strip()}")  # 진짜 오류
                else:
                    print(f"📝 서버 로그: {error.strip()}")  # INFO, DEBUG는 일반 로그로 출력
                sys.stdout.flush()  # 즉시 출력
            
            # 프로세스 종료 감지
            if process.poll() is not None:
                break

        process.stdout.close()
        process.stderr.close()


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
