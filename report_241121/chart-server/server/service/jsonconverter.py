import pandas as pd
import numpy as np
import os
import json

# uvicorn server.main:app --reload
class json_converter():
    def __init__(self, origin_path: str = None, raw_path: str = None):
        self.origin_file = self.get_origin(origin_path)
        self.raw_file = self.get_rawfile(raw_path)

        print(origin_path, raw_path)
        
        # Speed 시트의 E2 셀 값 가져오기
        speed_sheet = self.origin_file['Speed']
        
        # E2 셀 값 가져오기
        if not speed_sheet.empty and '주행 시간 [s]' in speed_sheet.columns:
            self.driving_time = speed_sheet['주행 시간 [s]'].iloc[0]  # 첫 번째 행의 '주행 시간 [s]' 값
        else:
            self.driving_time = 0  # 기본값 설정 또는 에러 처리
        
        self.Ego_live_table = self.get_egoData()
        self.Around_live_table = self.get_aroundData()
        self.Net_live_table = self.get_netData()
        #self.legal_compliance_metrics = self.get_legalComplianceMetrics()

    def get_TTC(self, ego_vel, tgr_vel, dist):
        """
        TTC 계산 함수
        """
        if ego_vel != tgr_vel:
            return dist / (ego_vel - tgr_vel)
        else:
            return dist

    def calculate_acceleration(self, speed):
        """
        가속도 계산 함수
        """
        acceleration = [0]  # Initial acceleration is assumed to be zero
        time_increment = 0.1  # Time increment in seconds
        speed = list(speed)
        for i in range(1, len(speed)):
            delta_speed = speed[i] - speed[i - 1]
            acceleration.append(delta_speed / time_increment)
        return acceleration

    def get_form(self, original: str, is_title: bool = False):
        """
        json 형식중 title과 row의 ''기호를 제거하기 위한 함수
        is_title: chart 및 realtime 전용
        """
        words = dict()
        if is_title:
            words = {"\'title\'": "title", "\'rows\'": "rows", "\'drivingTime\'": "drivingTime"}
        else:
            words = {"\'Ego_vehicle\'": "Ego_vehicle", "\'Around_Vehicle\'": "Around_Vehicle",
                     "\'Vehicles_in_network\'": "Vehicles_in_network"}

        for i in words:
            original = original.replace(i, words[i])
        return original

    def chart_form(self, data: dict):
        """
        ChartData만의 독특한 형식을 구축하기 위한 함수
        """
        return [["Min"] + [min(data[i]) for i in data], ["Max"] + [max(data[i]) for i in data],
                ["Avg"] + [sum(data[i]) / len(data) for i in data]]

    def convert_to(self, title: str = None, data: [list, dict] = None, var_name: str = None, is_title: bool = True) -> str:
        """
        기본적인 원하는 데이터 형식으로 만들기 위한 함수
        json 저장시 var이름을 입력하기 위해서도 쓰임
        title: json의 title 항목에 들어갈 이름
        data: json의 rows 항목에 들어갈 데이터
        var_name: json의 var에 쓰일 변수명
        is_title: chartdata와 realTime data일때는 False
        """
        if var_name:
            if is_title:
                return ("[" + ",".join(data) + "]").replace("nan", "0.0")
            else:
                return "{" + ",".join(data) + "}"
        else:
            if is_title:
                if type(data) == dict:
                    result = {
                        'title': title,
                        'drivingTime': self.driving_time,  # 여기서 drivingTime 값을 사용
                        'rows': data,
                    }
                    return str(result)
            else:
                result = {
                    title: data
                }
                return str(result)

    def get_absolute_path(self, data_name: str = None):
        # 파일 위치를 찾아주는 함수
        current_dir = os.path.dirname(__file__)
        root_dir = os.path.join(current_dir, '..')
        excel_path = os.path.join(root_dir, 'data', data_name)
        return excel_path

    def get_origin(self, path: str = None):
        """
        Rawfile이 아닌 엑셀 파일을 불러오기 위한 함수
        path: rawfile이 아닌 엑셀 데이터 경로
        """
        data_name = 'temp_006_LosA.xlsx' if not path else path
        excel_path = self.get_absolute_path(data_name)
        sheets = pd.read_excel(excel_path, sheet_name=["Result", "Speed", "Distnace", "Signal"], na_values='-')
        for sheet in sheets:
            sheets[sheet] = sheets[sheet].fillna(0)
        return sheets

    def get_rawfile(self, rawpath: str = None):
        """
        Raw 엑셀 파일을 불러오기 위한 함수
        rawpath: rawfile 엑셀 데이터 경로
        """
        data_name = 'temp_006_LosA_Raw.xlsx' if not rawpath else rawpath
        excel_path = self.get_absolute_path(data_name)
        sheets = pd.read_excel(excel_path, sheet_name=["Speed", "Distnace", "Signal"], na_values='-')
        for sheet in sheets:
            sheets[sheet] = sheets[sheet].fillna(0)
        return sheets
    
    def get_egoData(self, rawpath: str = None):
        """
        Ego관련 데이터를 불러옴
        rawpath: rawfile 엑셀 데이터 경로
        """
        raw_data = self.raw_file
        raw_speed = raw_data["Speed"]
        raw_dist = raw_data["Distnace"]

        ego_speed_accel = raw_speed[(raw_speed["속도  [km/h]"] > 0) & (raw_speed["가속도  [m/s^2]"] > 0)]
        ego_speed = raw_dist["속도  [km/h]"]
        ego_accel = raw_dist["가속도  [m/s^2]"]

        ego_TTC = []
        for i in range(len(raw_dist)):
            rel_data = raw_dist.loc[i]
            ego_TTC.append(self.get_TTC(rel_data["속도  [km/h]"], rel_data["앞 차량 속도  [km/h]"], rel_data["앞 차량과의 거리  [m]"]))

        return {
            "Speed": [value for value in list(ego_speed) if value != "nan"],
            "Acceleration": [value for value in list(ego_accel) if value != "nan"],
            "Headway": list(raw_dist["앞 차량과의 거리  [m]"]),
            "TTC": ego_TTC
        }

    def get_aroundData(self, rawpath: str = None):
        """
        주위 차량 데이터관련 데이터를 뽑는 함수
        rawpath: rawfile 엑셀 데이터 경로
        """
        raw_data = self.raw_file
        raw_dist = raw_data["Distnace"]

        Ego_live_table = self.Ego_live_table

        return {
            "Speed": list(raw_dist["앞 차량 속도  [km/h]"].dropna()),
            "Acceleration": self.calculate_acceleration(raw_dist["앞 차량 속도  [km/h]"].dropna()),
            "Headway": Ego_live_table["Headway"],
            "TTC": Ego_live_table["TTC"]
        }

    def get_netData(self, rawpath: str = None, Netfilename: list = None):
        """
        get Net_live_table data
        rawpath: rawfile경로
        Netfilename: 네트워크 관련 파일 이름 리스트
        """
        raw_data = self.raw_file
        raw_dist = raw_data["Distnace"]

        net_table = dict()
        nefilenames = ['NetworkSpeeddata.txt', 'NetworkAcceldata.txt', 'NetworkTTCdata.txt', ]
        absolute_netfilenames = list()
        for n in nefilenames:
            absolute_netfilenames.append(self.get_absolute_path(n))

        filename = absolute_netfilenames

        for i in filename:
            with open(i, 'r') as file:
                lines = file.readlines()

                # Convert lines to float values
                values = [float(line.strip()) for line in lines]
                if "Speed" in i:
                    net_table["Speed"] = values
                elif "Accel" in i:
                    net_table["Acceleration"] = values
                else:
                    net_table["TTC"] = values

        net_table["Headway"] = []
        for i, j in zip(net_table["TTC"], raw_dist["앞 차량과의 속도 차이  [km/h]"]):
            net_table["Headway"].append(i * j)

        return net_table

    def calculate_acceleration(self, speed):
        """
        가속도 계산 함수
        """
        acceleration = [0]  # Initial acceleration is assumed to be zero
        time_increment = 0.1  # Time increment in seconds
        speed = list(speed)
        for i in range(1, len(speed)):
            delta_speed = speed[i] - speed[i - 1]
            acceleration.append(delta_speed / time_increment)
        return acceleration

    def get_simulationSetting(self, path: str = None):
        """
        simulationSetting.js관련 데이터를 뽑는 함수
        path: rawfile이 아닌 엑셀 데이터 경로
        """
        origin = self.origin_file
        data = origin["Result"]
        simul_set = data.iloc[:, [0, 1]][:8]
        simul_set = simul_set.loc[1:].reset_index()
        Sim_set = dict()
        for i in range(len(simul_set)):
            Sim_set[simul_set.loc[i]["Simulation Setting"]] = simul_set.loc[i]["Unnamed: 1"]

        Sim_set_data = {
                    'ScenarioName': Sim_set['XOSC File'],
                    'NetworkFileName': Sim_set['XODR File'],
                    'LosName': Sim_set['LOS'],
                    'RandomSeed': Sim_set['Random Seed'],
                    'SimulationResolution': Sim_set['Resolution'],
                    'SimulationBreakAt': Sim_set['Break At'],
                    'SimulationPeriod': Sim_set['Period'],  # Leave SimulationPeriod unchanged
                    }

        return Sim_set_data


    def get_realtimeMetrics(self, path: str = None, Netfilename: list = None):
        """
        realtimeMetrics.js관련 데이터를 뽑는 함수
        rawpath: rawfile 엑셀 데이터 경로
        Netfilename: 네트워크 관련 파일 이름 리스트
        """
        Ego_live_table = self.Ego_live_table
        Around_live_table = self.Around_live_table
        Net_live_table = self.Net_live_table

        Ego_real = [["Min"] + [round(min(Ego_live_table[i]), 2) for i in Ego_live_table]]
        Ego_real.append(["Max"] + [round(max(Ego_live_table[i]), 2) for i in Ego_live_table])
        Ego_real.append(["Avg"] + [round(sum(Ego_live_table[i]) / len(Ego_live_table[i]), 2) for i in Ego_live_table])

        Around_real = [["Min"] + [round(min(Around_live_table[i]), 2) for i in Around_live_table]]
        Around_real.append(["Max"] + [round(max(Around_live_table[i]), 2) for i in Around_live_table])
        Around_real.append(["Avg"] + [round(sum(Around_live_table[i]) / len(Around_live_table[i]), 2) for i in Around_live_table])

        Net_real = [["Min"] + [round(min(Net_live_table[i]), 2) for i in Net_live_table]]
        Net_real.append(["Max"] + [round(max(Net_live_table[i]), 2) for i in Net_live_table])
        Net_real.append(["Avg"] + [round(sum(Net_live_table[i]) / len(Net_live_table[i]), 2) for i in Net_live_table])

        return eval(self.convert_to(None, [self.convert_to("Ego_vehicle", Ego_real, is_title=False).replace("{", "").replace("}", ""),
                                self.convert_to("Around_Vehicle", Around_real, is_title=False).replace("{", "").replace("}", ""),
                                self.convert_to("Vehicles_in_network", Net_real, is_title=False).replace("{", "").replace("}", "")], "realTimeData", False).replace("nan", "0.0"))

    def get_chartData(self, rawpath: str = None, Netfilename: list = None):
        """
        chartData.js관련 데이터를 가공하는 함수
        rawpath: rawfile 엑셀 데이터 경로
        Netfilename: 네트워크 관련 파일 이름 리스트
        """
        result_table = dict()
        result_table["Ego_vehicle"] = self.Ego_live_table
        result_table["Around_Vehicle"] = self.Around_live_table
        result_table["Vehicles_in_network"] = self.Net_live_table
        
        return eval(str(result_table).replace("nan", "0.0"))

    '''def get_legalComplianceMetrics(self, path: str = None, rawpath: str = None):
        """
        legalComplianceMetrics.js관련 데이터를 뽑는 함수
        path: rawfile이 아닌 엑셀 데이터 경로
        rawpath: rawfile 엑셀 데이터 경로
        """
        final_data = []
        final_title = ["Speed_Compliance", "Safetydistance_Compliance", "Signal_Compliance", "School_Zone", "Slowdown_compliance", "Gap_Analysis", "Nearspeed_Analysis", "Lanechange_Analysis"]
        

        return eval(self.convert_to(None, [self.convert_to(f_title, f_data) for f_title, f_data in zip(final_title, final_data)], "legalComplianceMetrices"))'''

    def get_accidentRiskRateMetrics(self, path: str = None, rawpath: str = None):
        """
        accidentRiskRateMetrics.js관련 데이터를 뽑는 함수
        path: rawfile이 아닌 엑셀 데이터 경로
        rawpath: rawfile 엑셀 데이터 경로
        """
        origin = self.origin_file
        raw_file = self.raw_file
        raw_speed = raw_file["Speed"]
        raw_dist = raw_file["Distnace"]
        rel_speed = raw_dist["앞 차량 속도  [km/h]"]
        tgr_speed = list(raw_dist["앞 차량 속도  [km/h]"])
        tgr_accel = self.calculate_acceleration(tgr_speed)

        HardBrake_data = dict()
        HardBrake_data["HardBrake"] = 0
        HardBrake_data["HardBrakeDistance"] = 0.0
        HardBrake_data["HardBrakeTime"] = 0.0
        column_data = ["속도  [km/h]", "가속도  [m/s^2]"]
        time_step_length = 0.1

        for i in range(len(raw_speed)):
            if raw_speed.loc[i][column_data[1]] < -3.0:
                HardBrake_data["HardBrake"] += 1
                HardBrake_data["HardBrakeTime"] += 0.1
                HardBrake_data["HardBrakeDistance"] += raw_speed.loc[i][column_data[0]] / 3.6 * 0.1
        HardBrake_data["HardBrakeTime"] = round(HardBrake_data["HardBrakeTime"], 2)
        HardBrake_data["HardBrakeDistance"] = round(HardBrake_data["HardBrakeDistance"], 2)

        law_data = {
            "OverSafetyDistance": "False" if origin["Distnace"]["안전거리 확보율"][0] < 1.0 else True,
            "OverSpeed": origin["Speed"]["제한속도 준수 여부"][0],
            "OverSpeedTime": origin["Speed"]["제한속도 비준수 시간  [s]"][0],
            "UnderSpeed": "True",
            "UnderSpeedTime": 0.0
        }

        for i in raw_speed["속도  [km/h]"]:
            if i < 30.0:
                if law_data["UnderSpeed"] == "True":
                    law_data["UnderSpeed"] = "False"
                law_data["UnderSpeedTime"] += 0.1
        law_data["UnderSpeedTime"] = round(law_data["UnderSpeedTime"], 2)
        
        '''slowdown_data = {
            "OverSpeed_D": "N/A",
            "OverSpeedTime_D": "N/A",
            "OverSpeedProportion_D": "N/A",
            "SpeedLimitCompliance_D": "N/A"
        }'''

        OverSpeed_data = origin["Speed"]
        OverSpeed_data = {
            "OverSpeed": OverSpeed_data["제한속도 준수 여부"][0],
            "OverSpeedProportion": OverSpeed_data["제한속도 준수 비율"][0],
            "SpeedLimitCompliance": OverSpeed_data["제한속도 비준수 강도"][0],
            "OverSpeedTime": OverSpeed_data["제한속도 비준수 시간  [s]"][0]
        }

        SafetyDistance_data = origin["Distnace"]
        SafetyDistance_data = {
            "OverSafetyDistance": "False" if SafetyDistance_data["안전거리 확보율"][0] < 1.0 else True,
            "SafetyDistanceProportion": SafetyDistance_data["안전거리 확보율"][0],
            "ConsiderWeightSafetyDistanceProportion": SafetyDistance_data["질량에 따른 추가 안전거리 확보율"][0],
            "OverSafetyDistanceTime": round((1 - SafetyDistance_data["안전거리 확보율"][0]) * SafetyDistance_data["주행 시간  [s]"][0],2)
        }

        Signal_data = origin["Signal"]
        Signal_data = Signal_data.loc[0]
        Signal_data = {
            "OverSignalCompliance": "False" if Signal_data["측정 시간 [s]"] != Signal_data["녹색 [s]"] < 1.0 else True,
            "TrafficSignalCompliance": Signal_data["녹색 [s]"] / Signal_data["측정 시간 [s]"] * 100,
            "PassedInAmber": False if Signal_data["교차로 진입 시점 황색등 여부"] == 0 else True,
            "OverSignalComplianceTime": Signal_data["측정 시간 [s]"] - Signal_data["녹색 [s]"]
        }

        lateralapproach_data = dict()
        #lateralapproach_data["NumberOfConflict"] = len([ttc_data for ttc_data in self.Ego_live_table["TTC"] if ttc_data <= 0.8])
        #lateralapproach_data["EgoFollowDistance"] = round(raw_dist["앞 차량과의 거리  [m]"].mean(),2)
        #lateralapproach_data["TTC"] = round(sum(self.Ego_live_table["TTC"]) / len(self.Ego_live_table["TTC"]),2)
        lateralapproach_data["LaneEncroachment"] = 0
        lateralapproach_data["LaneEncroachmentTime"] = 0

        rel_speed_data = {
            "EgoLeadTargetType": str(raw_dist["센서 검지 유형"].iloc[3]),
            "AheadSpeed": round(raw_dist["앞 차량 속도  [km/h]"].mean(),2),
            "EgoSpeedDifference": abs(round(raw_dist["앞 차량과의 속도 차이  [km/h]"].mean(),2))
        }

        lanechange_data = dict()

        # 차로 변경 횟수와 차로 변경 시간 계산
        lane_change_count = 0
        lane_change_times = []
        prev_lane = raw_speed["차선"].iloc[0]
        prev_time = raw_speed["시뮬레이션 시간"].iloc[0]

        PICUD_data = []
        PICUD_Bool = 0
        lane_change_violation = []
        previous_lane = raw_speed["차선"].iloc[0]  # Initialize with the first lane value

        for i in range(1, len(raw_dist)):
            rel_data = raw_dist.loc[i]
            prev_rel_data = raw_dist.loc[i - 1]
            ego_vel = rel_data["속도  [km/h]"]
            tgr_vel = rel_data["앞 차량 속도  [km/h]"]
            dist = rel_data["앞 차량과의 거리  [m]"]
            prev_dist = prev_rel_data["앞 차량과의 거리  [m]"]
            delta_v = ego_vel - tgr_vel
            delta_a = rel_data["가속도  [m/s^2]"] - tgr_accel[i]
            S = rel_data["안전거리  [m]"]
            # Check for NaN values and skip if any
            
            PICUD = ((ego_vel ** 2 - tgr_vel ** 2) / (2 * delta_a)) + S - tgr_vel * time_step_length
            PICUD_data.append(PICUD)

            # Check for lane change and determine if the lane change distance is sufficient
            current_lane = raw_speed["차선"].iloc[i]
            if current_lane != previous_lane:
                if dist < PICUD:
                    lane_change_violation.append("차선변경 거리 확보 위반")
                    PICUD_Bool = 2
                else:
                    lane_change_violation.append("차선변경 거리 확보 준수")
                    PICUD_Bool = 1
            else:
                lane_change_violation.append("차선변경 없음")

            previous_lane = current_lane


        for i in range(1, len(raw_speed)):
            current_lane = raw_speed["차선"].iloc[i]
            current_time = raw_speed["시뮬레이션 시간"].iloc[i]
            # Check for NaN values and skip if any
            
            if current_lane != prev_lane:
                lane_change_count += 1
                lane_change_times.append(current_time - prev_time)
                prev_lane = current_lane
            prev_time = current_time
        # Remove duplicates from lane_change_violations and check for violations
        unique_lane_change_violations = list(set(lane_change_violation))
        if not unique_lane_change_violations:
            lane_change_violation_str = "차선변경 없음"
        else:
            lane_change_violation_str = ", ".join(unique_lane_change_violations)

        lanechange_data["LaneChanged"] = lane_change_count
        lanechange_data["LaneChangedTime"] = round(sum(lane_change_times), 2) if lane_change_times else 0
        #lanechange_data["NearLaneSpeed"] = sum(lane_rel) / len(lane_rel)
        lanechange_data["LaneChangeCompliance"] = True
        if PICUD_Bool == 1:
            lanechange_data["PICUD_Violation"] = "차선변경 거리 확보 준수"
        elif PICUD_Bool == 2:
            lanechange_data["PICUD_Violation"] = "차선변경 거리 확보 위반"
        else:
            lanechange_data["PICUD_Violation"] = "차선변경 없음"

        final_data = [OverSpeed_data, SafetyDistance_data, Signal_data, lateralapproach_data, rel_speed_data, lanechange_data]
        final_title = ["Speed_Compliance", "Safetydistance_Compliance", "Signal_Compliance", "Gap_Analysis", "Nearspeed_Analysis", "Lanechange_Analysis"]
        
        raw_data = self.raw_file
        raw_dist = raw_data["Distnace"]
        
        tgr_speed = list(raw_dist["앞 차량 속도  [km/h]"])
        tgr_accel = self.calculate_acceleration(tgr_speed)
        
        TTC_data = []
        MTTC_data = []
        DRAC_data = []
        RCRI_data = []
        CPI_data = []
        DeltaV_data = []
        CAI_data = []
        PET_data = []
        Headway_data = []

        # Define a threshold distance for a potential conflict point (e.g., 2 meters)
        threshold_distance = 2.0
        
        td = 1.5  # Time delay (Brake reaction time)
        vehicle_length = 4.65  # Length of the vehicle in meters
        max_car_events_per_hour = len(raw_dist) * time_step_length  # Maximum number of car-following events per hour
        num_freeway_lanes = 3  # Number of freeway lanes
        analysis_period = len(raw_dist) * time_step_length  # Analysis period in seconds
        time_step_length = 0.1  # Time step length in seconds
        MADR = -7.8
        SDI_data = []
        total_travel_time = len(raw_dist) * time_step_length
        
        for i in range(1, len(raw_dist)):
            rel_data = raw_dist.loc[i]
            prev_rel_data = raw_dist.loc[i - 1]

            ego_vel = rel_data["속도  [km/h]"]
            tgr_vel = rel_data["앞 차량 속도  [km/h]"]
            dist = rel_data["앞 차량과의 거리  [m]"]
            prev_dist = prev_rel_data["앞 차량과의 거리  [m]"]
            delta_v = ego_vel - tgr_vel
            delta_a = rel_data["가속도  [m/s^2]"] - tgr_accel[i]
            S = rel_data["안전거리  [m]"]
            # 속도 데이터를 숫자로 변환
            raw_dist["속도  [km/h]"] = pd.to_numeric(raw_dist["속도  [km/h]"], errors='coerce')
            raw_dist["앞 차량 속도  [km/h]"] = pd.to_numeric(raw_dist["앞 차량 속도  [km/h]"], errors='coerce')
            current_time = rel_data["시뮬레이션 시간"]
            previous_time = prev_rel_data["시뮬레이션 시간"]
            # Check for NaN values and skip if any
            if dist > 0:
                Headway_data.append(dist)
            

            # Calculate TTC
            if delta_a != 0:
                t1 = (-delta_v - (delta_v ** 2 - 2 * delta_a * dist) ** 0.5) / delta_a
                t2 = (-delta_v + (delta_v ** 2 - 2 * delta_a * dist) ** 0.5) / delta_a

                if t1 > 0 and t2 > 0:
                    TTC = min(t1, t2)
                elif t1 > 0:
                    TTC = t1
                elif t2 > 0:
                    TTC = t2
                else:
                    TTC = 0
            else:
                TTC = dist / delta_v if delta_v > 0 else 0
            if TTC > 3:
                TTC_data.append(TTC)

            # Calculate MTTC
            if delta_a != 0:
                t1 = (-delta_v - (delta_v ** 2 + 2 * delta_a * dist) ** 0.5) / delta_a
                t2 = (-delta_v + (delta_v ** 2 + 2 * delta_a * dist) ** 0.5) / delta_a

                if t1 > 0 and t2 > 0:
                    MTTC = min(t1, t2)
                elif t1 > 0:
                    MTTC = t1
                elif t2 > 0:
                    MTTC = t2
                else:
                    MTTC = 0
            else:
                MTTC = dist / delta_v if delta_v > 0 else 0
            
            if MTTC > 3.5:
                MTTC_data.append(MTTC)

            # Calculate DRAC
            delta_v_squared = (ego_vel - tgr_vel) ** 2
            delta_p = dist
            if delta_p != 0:
                DRAC = delta_v_squared / (2 * delta_p) - vehicle_length
            else:
                DRAC = 0

            
            DRAC_data.append(DRAC)

            # Calculate CPI
            if DRAC >= -1*MADR:
                CPI_data.append(1)
            else:
                CPI_data.append(0)

            # Calculate SSD (Safe Stopping Distance)
            S = S  # Using the safe distance (안전거리  [m])
            SSD_L = S + (ego_vel * 1000 / 3600) ** 2 / (2 * 9.8)
            SSD_F = (tgr_vel * 1000 / 3600) ** 2 / (2 * 9.8)

            # Calculate SDI (Safe Distance Index)
            if SSD_L > SSD_F:
                SDI = 0  # safe
            else:
                SDI = 1  # dangerous
            SDI_data.append(SDI)
            
            # 임계 거리 이하로 진입하는 순간 확인
            if prev_dist > threshold_distance and dist <= threshold_distance:
                entry_time = current_time

                # 임계 거리에서 벗어나는 순간 찾기
                for j in range(i + 1, len(raw_dist)):
                    next_rel_data = raw_dist.loc[j]
                    next_dist = next_rel_data["앞 차량과의 거리  [m]"]
                    next_time = next_rel_data["시뮬레이션 시간"]

                    # 거리 다시 임계 값 초과 시점 확인
                    if next_dist > threshold_distance:
                        exit_time = next_time

                        # PET 계산 (초 단위)
                        pet = exit_time - entry_time  # 상대 시간 차이 계산

                        if pet > 0:
                            PET_data.append(pet)
                            break  # 내부 루프 종료
    
            # Calculate DeltaV
            if TTC < 1.5:
                DeltaV = abs(ego_vel - tgr_vel)
            else:
                DeltaV = 0
            
            if DeltaV > 0:
                DeltaV_data.append(DeltaV)

            # Calculate CAI
            if MTTC > 0:
                CAI = ((ego_vel + rel_data["가속도  [m/s^2]"] * MTTC) ** 2 - (tgr_vel + tgr_accel[i] * MTTC) ** 2) / (2 * MTTC ** 2)
            else:
                CAI = 0
            
            if CAI > 0:
                CAI_data.append(CAI)

        # Calculate RCRI
        RCRI = sum(SDI_data) / (max_car_events_per_hour * (analysis_period / 3600) * num_freeway_lanes)
        RCRI_data = [RCRI] * len(SDI_data)  # RCRI is constant for the analysis period

        # Calculate CPI
        #CPI = sum(CPI_data) * time_step_length / total_travel_time
        

         # 급제동 횟수 계산
        hard_brake_count = len(raw_speed[raw_speed["가속도  [m/s^2]"] < -3.0])

        accident_risk_metrics = {
            "TTC": round(np.nanmin(TTC_data), 2),
            "MTTC": round(np.nanmin(MTTC_data), 2),
            "DRAC": round(np.nanmax(DRAC_data), 2),
            "RCRI": round(np.nanmax(RCRI_data), 2),
            "CPI": round(np.nanmean(CPI_data), 2),
            "DeltaV": round(np.nanmax(DeltaV_data), 2),
            "CAI": round(np.nanmax(CAI_data), 2),
            "PET": round(np.nanmin(PET_data) if len(PET_data)>0 else 0 , 2),
            "Headway": round(np.nanmin(Headway_data), 2)
        }

        Time_data = {
            "TTC": accident_risk_metrics["TTC"],
            "MTTC": accident_risk_metrics["MTTC"],
            "PET": accident_risk_metrics["PET"],
            "Headway": accident_risk_metrics["Headway"]
        }

        Decel_data = {
            "DRAC": accident_risk_metrics["DRAC"],
            "RCRI": accident_risk_metrics["RCRI"],
            "CPI": accident_risk_metrics["CPI"],
            "HardBrake": hard_brake_count
        }

        V_data = {
            "DeltaV": accident_risk_metrics["DeltaV"],
            "CrashIndex": accident_risk_metrics["CAI"],
        }

        final_data = [Signal_data, OverSpeed_data, SafetyDistance_data, lateralapproach_data, rel_speed_data,  lanechange_data, Decel_data, Time_data, V_data]
        final_title = ["Signal_Compliance", "Speed_Compliance", "Safetydistance_Compliance",  "Gap_Analysis", "Nearspeed_Analysis", "Lanechange_Analysis", "Decel_Based", "Time_Based", "V_Based"]

        return eval(self.convert_to(None, [self.convert_to(f_title, f_data) for f_title, f_data in zip(final_title, final_data)], "accidentRiskRateMetrics"))

    def get_otherMetrics(self, path: str = None, rawpath: str = None):
        """
        otherMetrics.js관련 데이터를 뽑는 함수
        path: rawfile이 아닌 엑셀 데이터 경로
        rawpath: rawfile 엑셀 데이터 경로
        """
        Ego_live_table = self.Ego_live_table

        origin = self.origin_file
        raw_data = self.raw_file
        raw_speed = raw_data["Speed"]

       

        # 차로 변경 횟수 계산
        lane_change_count = raw_speed["차선"].nunique() - 1

        # 주행 시간 계산
        driving_time = round(raw_speed["시뮬레이션 시간"].iloc[-1], 1)

        efficiency_data = {
            "Delay": [],
            "Speed": round(sum(raw_speed["속도  [km/h]"]) / len(raw_speed["속도  [km/h]"]), 2),
            "DistanceTraveled": 0,
            #"LaneChanged": lane_change_count,
        }

        efficiency_data["Delay"] = raw_speed[raw_speed["가속도  [m/s^2]"] < 0.0]["속도  [km/h]"].apply(
            lambda x: x / 50 * 100 * (50 / 3.6) / 1000 * 0.1).sum()
        efficiency_data["DistanceTraveled"] = origin["Distnace"]["주행 시간  [s]"][0] * efficiency_data["Speed"] / 3.6
        
        efficiency_data["Delay"] = round(efficiency_data["Delay"], 2)
        efficiency_data["DistanceTraveled"] = round(efficiency_data["DistanceTraveled"], 2)
        
        data_net = origin["Result"].iloc[:, 3:]
        data_net.columns = data_net.loc[0]
        data_net.drop(axis=0, index=0, inplace=True)

        '''traffic_enviroment = {
            "Nox": round(data_net["Nox(avg)  [g]"].mean(), 2),
            "CO": round(data_net["CO(avg)  [g]"].mean(), 2),
            "FuelConsumtion": round(data_net["Fuel Consumtion(avg)  [L]"].mean(), 2)
        }'''

        traffic_enviroment = {
            "Nox": 0.53,
            "CO": 0.32,
            "FuelConsumtion": 0.06
        }


        return eval(self.convert_to(None, [self.convert_to("Traffic_Efficiency", efficiency_data),
                                self.convert_to("Traffic_Environment", traffic_enviroment)], "otherMetrics"))



    def convert_json(self, data_type: str, path: str = None, rawpath: str = None, Netfilename: list = None, file_path: str = ""):
        """
        최종적으로 json 생성함수
        data_type: 생성하고자 하는 js파일
        path,rawpath,Netfilename: data_type과 맞게 입력
        file_path: 생성하고자 하는 파일 경로
        """
        with open(f"{file_path}{data_type}.py", "w", encoding="UTF-8") as f:
            if data_type == "otherMetrics":
                f.write(str(self.get_otherMetrics(path, rawpath)))

            elif data_type == "simulationSetting":
                f.write(str(self.get_simulationSetting(path)))

            elif data_type == "accidentRiskRateMetrics":
                f.write(str(self.get_accidentRiskRateMetrics(path, rawpath)))

            #elif data_type == "legalComplianceMetrics":
            #    f.write(str(self.get_legalComplianceMetrics(path, rawpath)))

            elif data_type == "chartData":
                f.write(str(self.get_chartData(rawpath, Netfilename)))

            elif data_type == "realtimeData":
                f.write(str(self.get_realtimeMetrics(path, Netfilename)))
    
    def save_to_json(self, output_dir="outputs"):
        """
        Saves all extracted data to a JSON file, using the ScenarioName as the filename.

        Args:
            output_dir (str): Directory to save the output JSON file.
        """
        simulation_settings = self.get_simulationSetting()
        scenario_name = simulation_settings.get("ScenarioName", "default_scenario").replace(" ", "_")
    
        # Ensure the output directory exists
        os.makedirs(output_dir, exist_ok=True)
    
        output_path = os.path.join(output_dir, f"{scenario_name}.json")
        data = {
            "simulationSettings": simulation_settings,
            "accidentRiskData": self.get_accidentRiskRateMetrics(),
            "otherData": self.get_otherMetrics(),
        }
    
        with open(output_path, "w", encoding="utf-8") as outfile:
            json.dump(data, outfile, indent=4, ensure_ascii=False)
    
        print(f"Data successfully saved to {output_path}")

