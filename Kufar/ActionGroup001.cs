using System;
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
            //парсим главные категории в Бобруйске
            //ParseMainCategory(instance, project);
            ParsePodCategoryAvto(instance, project);

            return 0;
        }


        /// <summary>
        /// Прасим главные категории
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="p"></param>
        public static void ParseMainCategory(Instance instance, IZennoPosterProjectModel p)
        {
            //instance.ClearCookie();
            BD bd = new BD(connectBD);
            Tab tab = instance.ActiveTab;
            if ((tab.IsVoid) || (tab.IsNull)) return;
            if (tab.IsBusy) tab.WaitDownloading();
            tab.Navigate("https://www.kufar.by/listings?rgn=4&ar=12", "");
            if (tab.IsBusy) tab.WaitDownloading();

            string xPathColEl = "//div[@data-name = 'left_menu']//a";
            //парсим категории
            HtmlElementCollection colEl = tab.FindElementsByXPath(xPathColEl);


            foreach (var el in colEl)
            {
                string nameCategory = el.InnerText.Trim();
                string linkCategory = el.GetAttribute("href");

                Send.InfoToLog(p, $"{nameCategory} - {linkCategory}");

                string query = $"INSERT INTO categories (`mainСategory`, `linkMainСategory`, `status`) VALUES ('{nameCategory}', '{linkCategory}', 'true')";

                try
                {
                    bd.InsertUpdateBD(query);
                }
                catch (Exception ex)
                {
                    Send.InfoToLog(p, ex.Message);
                }

            }


        }


        public static void ParsePodCategoryAvto(Instance instance, IZennoPosterProjectModel p)
        {
            //instance.ClearCookie();
            BD bd = new BD(connectBD);
            Tab tab = instance.ActiveTab;

            tab.NavigateAndWait("https://auto.kufar.by/", 2000);

            string query = "INSERT INTO categories (`mainСategory`, `category1`, `linkСategory1`, `status`) VALUES ('Авто и транспорт','{0}', '{1}', '---')";
            string xpath = "//div[contains(@class, 'swiper-wrapper')]/a";
            AddBdCategory(instance, p, xpath, query);




        }




        static void AddBdCategory(Instance instance, IZennoPosterProjectModel p, string xPathColEl, string queryFormat)
        {
            BD bd = new BD(connectBD);
            //парсим категории
            HtmlElementCollection colEl = instance.ActiveTab.FindElementsByXPath(xPathColEl);


            foreach (var el in colEl)
            {
                string nameCategory = el.InnerText.Trim();
                string linkCategory = el.GetAttribute("href").Trim() + "&typ=sell&sort=lst.d&size=30&cur=BYR&rgn=4&ar=12";

                Send.InfoToLog(p, $"{nameCategory} - {linkCategory}");

                string query = String.Format(queryFormat, nameCategory, linkCategory);

                try
                {
                    bd.InsertUpdateBD(query);
                }
                catch (Exception ex)
                {
                    Send.InfoToLog(p, ex.Message);
                }

            }


        }

    }
}