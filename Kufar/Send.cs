using System.Diagnostics;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    public class Send
    {



        public static void InfoToLog(IZennoPosterProjectModel project, string text, bool log = false)
        {

            //project.SendInfoToLog(text, log);

            Debug.WriteLine(text);
        }
    }
}
