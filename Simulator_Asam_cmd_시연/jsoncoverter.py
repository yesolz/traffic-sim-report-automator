import pandas as pd
import json
import numpy as np

origin=pd.read_excel(r"C:\\Users\\user\\Documents\\Simulator\\Resources\\vissim\\temp_015_LosA.xlsx",sheet_name=["Result","Speed","Distnace","Signal"])

raw_file=pd.read_excel("C:\\Users\\user\\Documents\\Simulator\\Resources\\vissim\\temp_015_LosA_Raw.xlsx",sheet_name=["Speed","Distnace","Signal"])

final=[]

def get_TTC(ego_vel,tgr_vel,dist):
    if ego_vel!=tgr_vel:
        return dist/(ego_vel-tgr_vel)
    else:
        return dist

data=origin["Result"]
simul_set=data.iloc[:,[0,1]][:7]
simul_set=simul_set.loc[1:].reset_index()
Sim_set_data=dict()
for i in range(len(simul_set)):
    Sim_set_data[simul_set.loc[i]["Simulation Setting"]]=simul_set.loc[i]["Unnamed: 1"]

final.append(Sim_set_data)

OverSpeed_data=origin["Speed"]
OverSpeed_data={
    "OverSpeed": OverSpeed_data["제한속도 준수 여부"][0],
    "OverSpeedProportion": OverSpeed_data["제한속도 준수 비율"][0],
    "SpeedLimitCompliance": OverSpeed_data["제한속도 비준수 강도"][0],
    "OverSpeedTime": OverSpeed_data["제한속도 비준수 시간  [s]"][0]
}

final.append(OverSpeed_data)

SafetyDistance_data=origin["Distnace"]
SafetyDistance_data={
    "OverSafetyDistance": False if SafetyDistance_data["안전거리 확보율"][0] <1.0 else True,
    "SafetyDistanceProportion": SafetyDistance_data["안전거리 확보율"][0],
    "ConsiderWeightSafetyDistanceProportion": SafetyDistance_data["질량에 따른 추가 안전거리 확보율"][0],
    "OverSafetyDistanceTime": (1- SafetyDistance_data["안전거리 확보율"][0])*SafetyDistance_data["주행 시간  [s]"][0]
}

final.append(SafetyDistance_data)

Signal_data=origin["Signal"]
Signal_data=Signal_data.loc[0]

Signal_data={
    "OverSignalCompliance": False if Signal_data["측정 시간 [s]"] != Signal_data["녹색 [s]"] <1.0 else True,
    "TSC": Signal_data["녹색 [s]"]/Signal_data["측정 시간 [s]"]*100,
    "PassedInAmber": Signal_data["교차로 진입 시점 황색등 여부"],
    "OverSignalComplianceTime": Signal_data["측정 시간 [s]"]-Signal_data["녹색 [s]"]
}

final.append(Signal_data)

origin_raw=raw_file
raw_file=raw_file["Speed"]
column_data=["속도  [km/h]","가속도  [m/s^2]"]

ego_speed_accel=raw_file[(raw_file["속도  [km/h]"]>0) &(raw_file["가속도  [m/s^2]"]>0)]
ego_speed=ego_speed_accel["속도  [km/h]"]
ego_accel=ego_speed_accel["가속도  [m/s^2]"]

graph_category={"Speed":["0_10","10_20","20_30","30_40","40_"],
"Acceleration":["_-4","-4_0","0_4","4_8","8_"],
"Headway":["0_20","20_40","40_60","60_80","80_"],
"TTC":["0_1.2","1.2_2.4","2.4_3.6","3.6_4.8","4.8_"]}

Ego_live_table={
    "Speed":[ego_speed.min(),ego_speed.mean(),ego_speed.max()],
    "Acceleration":[ego_accel.min(),ego_accel.mean(),ego_accel.max()],
    "Headway":[],
    "TTC":[]
}

Ego_live_graph={
    "Speed":[],
    "Acceleration":[],
    "Headway":[],
    "TTC":[]
}

final.append(Ego_live_graph)

HardBrake_data=dict()
HardBrake_data["HardBrake"]=0
HardBrake_data["HardBrakeDistance"]=0.0
HardBrake_data["HardBrakeTime"]=0.0
for i in range(len(raw_file)):
    if raw_file.loc[i][column_data[1]]<-3.0:
        HardBrake_data["HardBrake"]+=1
        HardBrake_data["HardBrakeTime"]+=0.1
        HardBrake_data["HardBrakeDistance"]+=raw_file.loc[i][column_data[0]]/3.6*0.1

final.append(HardBrake_data)

law_data={
    "OverSafetyDistance":SafetyDistance_data["OverSafetyDistance"],
    "OverSpeed":OverSpeed_data["OverSpeed"],
    "OverSpeedTime":OverSpeed_data["OverSpeedTime"],
    "UnderSpeed":True,
    "UnderSpeedTime":0.0
}

for i in raw_file["속도  [km/h]"]:
    if i<30.0:
        if law_data["UnderSpeed"] == True:
            law_data["UnderSpeed"]=False
        law_data["UnderSpeedTime"]+=0.1

final.append(law_data)

raw_dist=origin_raw["Distnace"]
raw_dist=raw_dist.dropna(subset=["앞 차량 속도  [km/h]","앞 차량과의 거리  [m]"])
raw_dist.reset_index(inplace=True)

aheaddist_data={
    "NumberOfConflict":[],
    "TTC":[],
    "EgoFollowDistance":[]
}
for i in range(len(raw_dist)):
    rel_data=raw_dist.loc[i]
    Ego_live_table["TTC"].append(get_TTC(rel_data["속도  [km/h]"],rel_data["앞 차량 속도  [km/h]"],rel_data["앞 차량과의 거리  [m]"]))
    
aheaddist_data["EgoFollowDistance"]=raw_dist["앞 차량과의 거리  [m]"].mean()
aheaddist_data["NumberOfConflict"]= len([ttc_data for ttc_data in Ego_live_table["TTC"] if ttc_data<=0.8])
aheaddist_data["TTC"]= sum(Ego_live_table["TTC"])/len(Ego_live_table["TTC"])

final.append(aheaddist_data)

Ego_live_table["Headway"]=[raw_dist["앞 차량과의 거리  [m]"].min(),raw_dist["앞 차량과의 거리  [m]"].mean(),raw_dist["앞 차량과의 거리  [m]"].max()]

Ego_live_table["TTC"]=[min(Ego_live_table["TTC"]),aheaddist_data["TTC"],max(Ego_live_table["TTC"])]

final.append(Ego_live_table)

rel_speed_data={
    "EgoLeadTargetType":"VEHICLE",
    "AheadSpeed":raw_dist["앞 차량 속도  [km/h]"].mean(),
    "EgoSpeedDifference":raw_dist["앞 차량과의 속도 차이  [km/h]"].mean()
}

final.append(rel_speed_data)

rel_speed=raw_dist["앞 차량 속도  [km/h]"]

rel_live_table={
    "Speed":[rel_speed.min(),rel_speed.mean(),rel_speed.max()],
    "Acceleration":[ego_accel.min(),ego_accel.mean(),ego_accel.max()],
    "Headway":Ego_live_table["Headway"],
    "TTC":Ego_live_table["TTC"]
}

rel_live_graph={
    "Speed":[],
    "Acceleration":[],
    "Headway":[],
    "TTC":[]
}

final.append(rel_live_table)
final.append(rel_live_graph)

efficiency_data={
    "Delay":[],
    "Speed":Ego_live_table["Speed"][1],
    "Queuelen":0,
    "DistanceTraveled":0
    }
efficiency_data["Delay"]=raw_file[raw_file["가속도  [m/s^2]"]<0.0]["속도  [km/h]"].apply(lambda x: x/50*100*(50/3.6)/1000*0.1).sum()
efficiency_data["DistanceTraveled"]= origin["Distnace"]["주행 시간  [s]"][0]*efficiency_data["Speed"]/3.6

final.append(efficiency_data)

print(final)