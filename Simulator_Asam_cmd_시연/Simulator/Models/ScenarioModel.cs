using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Simulator.Models
{
    /// <summary>
    /// OpenSCENARIO 데이터를 불러와서 Models.DTO에 있는 데이터 형태와 결합하여
    /// 메모리에서 값을 읽어와 사용함
    /// </summary>
    public class ScenarioModel
    {
        /// <summary>
        /// OpenSCENARIO 개체, 명시적 형변환으로 해당 개체를 변환하여 사용해야함
        /// </summary>
        public object OpenSCENARIO { get; init; }

        public object Catalog { get; set; }

        /// <summary>
        /// 메인 버전
        /// </summary>
        public int MajorVersion { get; init; }
        /// <summary>
        /// 서브 버전
        /// </summary>
        public int MinorVersion { get; init; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="major">메인 버전</param>
        /// <param name="minor">서브 버전</param>
        /// <param name="scenarioFileFullName">경로, 확장자를 포함한 파일명(.xosc)</param>
        /// <exception cref="Exception">반환 실패시 에러</exception>
        public ScenarioModel(int major, int minor, string scenarioFileFullName)
        {
            MajorVersion = major;
            MinorVersion = minor;

            switch (major, minor)
            {
                case (1, 0): OpenSCENARIO = InitOpenScenario_1_0(scenarioFileFullName); break;
                case (1, 1): OpenSCENARIO = InitOpenScenario_1_1(scenarioFileFullName); break;
                case (1, 2): OpenSCENARIO = InitOpenScenario_1_2(scenarioFileFullName); break;
                case (1, 3): OpenSCENARIO = InitOpenScenario_1_3(scenarioFileFullName); break;
                //case (-1, 0): OpenSCENARIO = InitCatalog_1_0(scenarioFileFullName); break;
                default: throw new Exception("Not identified version scenario imported");
            }
        }

        /// <summary>
        /// OpenSCENARIO_1_0 형태로 데이터를 변환
        /// </summary>
        /// <param name="scenarioFullFileName">경로, 확장자를 포함한 파일명(.xosc)</param>
        /// <returns></returns>
        private DTO.OpenSCENARIO_1_0.OpenScenario InitOpenScenario_1_0(string scenarioFullFileName)
        {
            XDocument doc = XDocument.Parse(File.ReadAllText(scenarioFullFileName));
            XmlSerializer sz = new XmlSerializer(typeof(DTO.OpenSCENARIO_1_0.OpenScenario));
            FileStream stream = File.Open(scenarioFullFileName, FileMode.Open);

            DTO.OpenSCENARIO_1_0.OpenScenario scenario = (DTO.OpenSCENARIO_1_0.OpenScenario)sz.Deserialize(stream)!;      
            stream.Close();

            Catalog = InitCatalog_1_0( @"\VehicleCatalog.xosc");

            return scenario;
        }

        private DTO.Catalog.OpenSCENARIO InitCatalog_1_0(string scenarioFullFileName)
        {
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Simulator";
            string resourcePath = defaultPath + @"\Resources" + @"\Catalogs_CARLA";
            scenarioFullFileName = resourcePath + scenarioFullFileName;
            XDocument doc = XDocument.Parse(File.ReadAllText(scenarioFullFileName));
            XmlSerializer sz = new XmlSerializer(typeof(DTO.Catalog.OpenSCENARIO));
            FileStream stream = File.Open(scenarioFullFileName, FileMode.Open);
            DTO.Catalog.OpenSCENARIO catalog = (DTO.Catalog.OpenSCENARIO)sz.Deserialize(stream)!;
            
            stream.Close();

            return catalog;
        }

        /// <summary>
        /// OpenSCENARIO_1_1 형태로 데이터를 변환
        /// </summary>
        /// <param name="scenarioFullFileName">경로, 확장자를 포함한 파일명(.xosc)</param>
        /// <returns></returns>
        private DTO.OpenSCENARIO_1_1.OpenScenario InitOpenScenario_1_1(string scenarioFullFileName)
        {
            XDocument doc = XDocument.Parse(File.ReadAllText(scenarioFullFileName));
            XmlSerializer sz = new XmlSerializer(typeof(DTO.OpenSCENARIO_1_1.OpenScenario));
            FileStream stream = File.Open(scenarioFullFileName, FileMode.Open);

            DTO.OpenSCENARIO_1_1.OpenScenario scenario = (DTO.OpenSCENARIO_1_1.OpenScenario)sz.Deserialize(stream)!;

            stream.Close();

            return scenario;
        }

        /// <summary>
        /// OpenSCENARIO_1_2 형태로 데이터를 변환
        /// </summary>
        /// <param name="scenarioFullFileName">경로, 확장자를 포함한 파일명(.xosc)</param>
        /// <returns></returns>
        private DTO.OpenSCENARIO_1_2.OpenScenario InitOpenScenario_1_2(string scenarioFullFileName)
        {
            XDocument doc = XDocument.Parse(File.ReadAllText(scenarioFullFileName));
            XmlSerializer sz = new XmlSerializer(typeof(DTO.OpenSCENARIO_1_2.OpenScenario));
            FileStream stream = File.Open(scenarioFullFileName, FileMode.Open);

            DTO.OpenSCENARIO_1_2.OpenScenario scenario = (DTO.OpenSCENARIO_1_2.OpenScenario)sz.Deserialize(stream)!;

            stream.Close();

            return scenario;
        }

        private DTO.OpenSCENARIO_1_2.OpenScenario InitOpenScenario_1_3(string scenarioFullFileName)
        {
            XDocument doc = XDocument.Parse(File.ReadAllText(scenarioFullFileName));
            XmlSerializer sz = new XmlSerializer(typeof(DTO.OpenSCENARIO_1_2.OpenScenario));
            FileStream stream = File.Open(scenarioFullFileName, FileMode.Open);

            DTO.OpenSCENARIO_1_2.OpenScenario scenario = (DTO.OpenSCENARIO_1_2.OpenScenario)sz.Deserialize(stream)!;

            stream.Close();

            return scenario;
        }
    }

    /// <summary>
    /// 시나리오에 사용되는 파일의 정보를 저장
    /// </summary>
    public class ScenarioFileInfo
    {
        /// <summary>
        /// 실행 Index
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// xosc 파일 정보
        /// </summary>
        private FileInfo? _scenarioFileInfo { get; init; }
        /// <summary>
        /// xodr 파일 정보
        /// </summary>
        private FileInfo? _networkFileInfo { get; init; }

        /// <summary>
        /// 파일명 요청 시 _scenarioFileInfo.Name 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string ScenarioFileName
        {
            get
            {
                if (_scenarioFileInfo is null)
                {
                    return "";
                }
                return _scenarioFileInfo.Name.Replace(_scenarioFileInfo.Extension, "");
            }
        }

        /// <summary>
        /// 파일명 요청 시 _scenarioFileInfo.FullName 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string ScenarioFileFullName
        {
            get
            {
                if (_scenarioFileInfo is null)
                {
                    return "";
                }
                return _scenarioFileInfo.FullName;
            }
        }

        /// <summary>
        /// 파일명 요청 시 _networkFileInfo.Name 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string NetworkFileName
        {
            get
            {
                if (_networkFileInfo is null)
                {
                    return "";
                }
                return _networkFileInfo.Name.Replace(_networkFileInfo.Extension, "");
            }
        }

        /// <summary>
        /// 파일명 요청 시 _networkFileInfo.FullName 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string NetworkFileFullName
        {
            get
            {
                if (_networkFileInfo is null)
                {
                    return "";
                }

                return _networkFileInfo.FullName;
            }
        }

        /// <summary>
        /// 메인 버전
        /// </summary>
        public int MajorVersion { get; init; }
        /// <summary>
        /// 서브 버전
        /// </summary>
        public int MinorVersion { get; init; }
        /// <summary>
        /// 데이터 개체
        /// </summary>

        public ScenarioModel Scenario { get; init; }
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="scenarioFileInfo">*.xosc 파일 정보</param>
        /// <param name="networkFileInfo">*.xodr 파일 정보</param>
        /// <param name="major">메인 버전</param>
        /// <param name="minor">서브 버전</param>
        /// <param name="index">순서</param>

        public ScenarioFileInfo(FileInfo scenarioFileInfo, FileInfo networkFileInfo, int major, int minor, int index)
        {
            _scenarioFileInfo = scenarioFileInfo;
            _networkFileInfo = networkFileInfo;
            MajorVersion = major;
            MinorVersion = minor;

            Scenario = new ScenarioModel(major, minor, ScenarioFileFullName);
            Index = index;
        }
    }

    /// <summary>
    /// 시나리오 파일 목록
    /// </summary>
    public class ScenarioFileList : ObservableCollection<ScenarioFileInfo>
    {

    }


}
