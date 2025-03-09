using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.ViewModels
{
    /// <summary>
    /// Network 편집 화면을 제공<br/>
    /// ** Vissim Network 생성 및 vissim을 확인하는 방향으로 전환, 사용하지 않음
    /// </summary>
    public class NetworkViewModel : BaseViewModel
    {
        public string FileName { get; private set; }
        public NetworkViewModel()
        {
            FileName = "";
        }

        public NetworkViewModel(string fileName)
        {
            FileName = fileName;
        }
    }
}
