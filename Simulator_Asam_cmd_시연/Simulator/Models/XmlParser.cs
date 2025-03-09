using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Simulator.Models
{
    /// <summary>
    /// Xml파일(.xosc, .xodr)을 읽어 사용할 경우에 사용하기위해 만듬<br/>
    /// 로직 내부에서 직접 파일을 참조하여 사용하는 경우도 존재함
    /// </summary>
    internal class XmlParser
    {
        /// <summary>
        /// .xsoc 파일에서 .xodr 파일의 경로를 가져옴
        /// </summary>
        /// <param name="fullFileName">경로 + 확장자를 포함한 .xsoc파일</param>
        /// <returns></returns>
        public string GetNetworkFileName(string fullFileName)
        {
            // XML 문서 읽어옴
            XDocument xmlDoc = XDocument.Load(fullFileName);

            return xmlDoc.Element("OpenSCENARIO")!.Element("RoadNetwork")!.Element("LogicFile")!.Attribute("filepath")!.Value;

			/* 위와 동일한 코드..
            foreach (XElement element in xmlDoc!.Root!.Descendants())
            {
                if (element.Name.ToString().Equals("RoadNetwork"))
                {
                    // RoadNetwork 
                    foreach (XElement item in element.Descendants())
                    {
                        if (item.Name.ToString().Equals("LogicFile"))
                        {
                            foreach (XAttribute attribute in item.Attributes())
                            {
                                if (attribute.Name.ToString().Equals("filepath"))
                                {
                                    return attribute.Value;
                                }
                            }
                        }
                    }
                }
            }

            return "";
            */
		}

		/// <summary>
		/// .xsoc 파일에서 .xodr 파일의 경로를 가져옴
		/// </summary>
		/// <param name="fullFileName">경로 + 확장자를 포함한 .xsoc파일</param>
		/// <returns></returns>
		public (int, int) GetScenarioVersion(string fullFileName)
        {
			XDocument xmlDoc = XDocument.Load(fullFileName);
            int major = 0;
            int minor = 0;
            int scenario_index = 0;


            XElement fileHeaderElement = xmlDoc.Element("OpenSCENARIO")!.Element("FileHeader")!;

            foreach (var attribute in fileHeaderElement.Attributes())
            {
				if ( attribute.Name.ToString().Equals("revMajor") )
				{
					major = Convert.ToInt32(attribute.Value);
				}
				else if ( attribute.Name.ToString().Equals("revMinor") )
				{
					minor = Convert.ToInt32(attribute.Value);
				}
                else if (attribute.Name.ToString().Equals("description"))
                {
                    if(attribute.Value.Contains("cut-in") )  Shared.ScenarioIndex = 0;
                    else if (attribute.Value.Contains("cut-off")) Shared.ScenarioIndex = 1;
                    else if (attribute.Value.Contains("bicycle_slow_evasion")) Shared.ScenarioIndex = 2;
                    else if (attribute.Value.Contains("accident_slow_evasion")) Shared.ScenarioIndex = 3;
                    else if (attribute.Value.Contains("collision_slow_evasion")) Shared.ScenarioIndex = 4;
                    else if (attribute.Value.Contains("intersection_straight")) Shared.ScenarioIndex = 5;
                    else if (attribute.Value.Contains("좌회전")) Shared.ScenarioIndex = 6;
                    else if (attribute.Value.Contains("intersection_rightTurn")) Shared.ScenarioIndex = 7;
                    else if (attribute.Value.Contains("roundabout_SlowDown")) Shared.ScenarioIndex = 8;
                }
			}

            return (major, minor);
            /* 위와 동일한 코드
            foreach (XElement element in xmlDoc!.Root!.Descendants())
            {
                if (element.Name.ToString().Equals("FileHeader"))
                {
                    foreach (XAttribute attribute in element!.Attributes())
                    {
                        if (attribute.Name.ToString().Equals("revMajor"))
                        {
                            major = Convert.ToInt32(attribute.Value);
                        }
                        else if (attribute.Name.ToString().Equals("revMinor"))
                        {
							minor = Convert.ToInt32(attribute.Value);
						}
                    }
                }
            }

            return (major, minor);
            */
		}
    }

    /// <summary>
    /// Vissim의 Evaluation - VehicleRecord에서 Attributes...를 설정할 때 사용하는 구조체<br/>
    /// COM Interface에서는 설정 가능한 방법을 못찾음<br/>
    /// .inpx 파일을 열어보면 xml 형태로 되어있음<br/>
    /// 동일한 형태로 설정하도록 되어있어, 해당 데이터 구조체를 만듬
    /// </summary>
	public class VissimAttributeSelection
	{
        /// <summary>
        /// Attribute ID
        /// </summary>
		public string attributeID { get; set; }
        /// <summary>
        /// 소수점 설정
        /// </summary>
		public int decimals { get; set; }
        /// <summary>
        /// 포현 포멧
        /// </summary>
		public string format { get; set; }
        /// <summary>
        /// 단위 표시 여부
        /// </summary>
		public bool showUnits { get; set; }

		public VissimAttributeSelection(string attributeId, int decimals, string format, bool showUnits)
		{
			this.attributeID = attributeId;
			this.decimals = decimals;
			this.format = format;
			this.showUnits = showUnits;
		}
	}
}
