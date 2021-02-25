using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    /// <summary>
    /// Класс выполнения группы действий
    /// </summary>
    internal class ActionGroup001
    {
        public const string connectBD = "Server=localhost;Database=kufar;Uid=root;Pwd=vertrigo;Port=3306;CharSet=utf8;Pooling=True;";


        /// <summary>
        /// Метод выполнения группы действий
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения группы действий</returns>
        public static int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            //instance.ClearCookie();
            BD bd = new BD(connectBD);
            Tab tab = instance.ActiveTab;
            if ((tab.IsVoid) || (tab.IsNull)) return -1;
            if (tab.IsBusy) tab.WaitDownloading();
            //tab.Navigate("https://www.kufar.by/listings", "");
            if (tab.IsBusy) tab.WaitDownloading();

            string xPathColEl = "//div[@data-name = 'left_menu']//a";
            //парсим категории
            HtmlElementCollection colEl = tab.FindElementsByXPath(xPathColEl);


            foreach (var el in colEl)
            {
                string nameCategory = el.InnerText.Trim();
                string linkCategory = el.GetAttribute("href");

                Send.InfoToLog(project, $"{nameCategory} - {linkCategory}");
            }



            return 0;
        }
    }
}