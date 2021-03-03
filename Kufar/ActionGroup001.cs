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
            BD bd = new BD(connectBD);
            while (true)
            {
                string query = "SELECT id, `mainСategory`, `linkMainСategory`" +
                    " FROM categories WHERE `status`='---' AND linkMainСategory != '---' LIMIT 1;";

                string res = bd.SelectBD(query);

                if (string.IsNullOrWhiteSpace(res)) return 0;


                string[] arrRes = res.Split('|');


                int idCategory = int.Parse(arrRes[0]);
                string name = arrRes[1];
                string url = arrRes[2];

                string xpath = "//div[@data-name = 'left_menu']//li//a";


                query = $"UPDATE categories	SET `status`='ok' WHERE id='{idCategory}';";
                bd.InsertUpdateBD(query);



                ParsePodCategoryUniversal(instance, project, xpath, url, idCategory, name);

            }


            return 0;
        }

        #region
        /// <summary>
        /// Прасим главные категории
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="p"></param>
        //public static void ParseMainCategory(Instance instance, IZennoPosterProjectModel p)
        //{
        //    //instance.ClearCookie();
        //    BD bd = new BD(connectBD);
        //    Tab tab = instance.ActiveTab;
        //    if ((tab.IsVoid) || (tab.IsNull)) return;
        //    if (tab.IsBusy) tab.WaitDownloading();
        //    tab.Navigate("https://www.kufar.by/listings?rgn=4&ar=12", "");
        //    if (tab.IsBusy) tab.WaitDownloading();

        //    string xPathColEl = "//div[@data-name = 'left_menu']//a";
        //    //парсим категории
        //    HtmlElementCollection colEl = tab.FindElementsByXPath(xPathColEl);


        //    foreach (var el in colEl)
        //    {
        //        string nameCategory = el.InnerText.Trim();
        //        string linkCategory = el.GetAttribute("href");

        //        Send.InfoToLog(p, $"{nameCategory} - {linkCategory}");

        //        string query = $"INSERT INTO categories (`mainСategory`, `linkMainСategory`, `status`) VALUES ('{nameCategory}', '{linkCategory}', 'true')";

        //        try
        //        {
        //            bd.InsertUpdateBD(query);
        //        }
        //        catch (Exception ex)
        //        {
        //            Send.InfoToLog(p, ex.Message);
        //        }

        //    }


        //}


        //public static void ParsePodCategoryAvto(Instance instance, IZennoPosterProjectModel p)
        //{
        //    //instance.ClearCookie();
        //    BD bd = new BD(connectBD);
        //    Tab tab = instance.ActiveTab;

        //    tab.NavigateAndWait("https://auto.kufar.by/listings?cat=2010&typ=sell&sort=lst.d&size=30&cur=BYR&rgn=4&ar=12", 2000);

        //    //string query = "INSERT INTO categories (`mainСategory`, `category1`, `linkСategory1`, `status`) VALUES ('Авто и транспорт','{0}', '{1}', '---')";
        //    string query = "INSERT INTO categories (`mainСategory`, `linkMainСategory`, idCategory , `status`) VALUES ('{0}', '{1}', 4 ,'true')";


        //    string xpath = "//div[contains(@class, 'swiper-wrapper')]/a";
        //    AddBdCategory(instance, p, xpath, query, 0);




        //}


        //public static void ParsePodCategoryBitTexnica(Instance instance, IZennoPosterProjectModel p)
        //{
        //    //instance.ClearCookie();
        //    BD bd = new BD(connectBD);
        //    Tab tab = instance.ActiveTab;

        //    tab.NavigateAndWait("https://www.kufar.by/listings?prn=15000&rgn=4&ar=12", 2000);

        //    string query = "INSERT INTO categories (`mainСategory`, `category1`, `linkСategory1`, `status`) VALUES ('Бытовая техника','{0}', '{1}', '---')";
        //    string xpath = "//div[@data-name = 'left_menu']//li//a";
        //    AddBdCategory(instance, p, xpath, query);




        //}

        //public static void ParsePodCategoryKompTexnica(Instance instance, IZennoPosterProjectModel p)
        //{
        //    //instance.ClearCookie();
        //    BD bd = new BD(connectBD);
        //    Tab tab = instance.ActiveTab;
        //    string mainurl = "https://www.kufar.by/listings?prn=16000&rgn=4&ar=12";

        //    tab.NavigateAndWait(mainurl, 2000);

        //    string query = "INSERT INTO categories (`mainСategory`, `category1`, `linkСategory1`, `status`) VALUES ('Компьютерная техника','{0}', '{1}', '---')";
        //    string xpath = "//div[@data-name = 'left_menu']//li//a";

        //    AddBdCategory(instance, p, xpath, query, mainurl);




        //}

        #endregion

        public static void ParsePodCategoryUniversal(Instance instance, IZennoPosterProjectModel p, string xpath, string url, int idCategory, string name)
        {
            //instance.ClearCookie();
            BD bd = new BD(connectBD);
            Tab tab = instance.ActiveTab;


            tab.NavigateAndWait(url, 500);

            //string query = "INSERT INTO categories (`mainСategory`, `category1`, `linkСategory1`, `status`) VALUES ('" + name + "','{0}', '{1}', '---')";

            string query = "INSERT INTO categories (`mainСategory`, idCategory,  `linkMainСategory`, `podCategory` ) VALUES ('{0}', '{1}', '{2}', '{3}')";

            //string xpath = "//div[@data-name = 'left_menu']//li//a";

            AddBdCategory(instance, p, xpath, query, idCategory, name);




        }


        static void AddBdCategory(Instance instance, IZennoPosterProjectModel p, string xPathColEl, string queryFormat, int idCategory, string name)
        {
            BD bd = new BD(connectBD);
            //парсим категории
            HtmlElementCollection colEl = instance.ActiveTab.FindElementsByXPath(xPathColEl);


            foreach (var el in colEl)
            {
                string nameCategory = el.InnerText.Trim();
                string linkCategory = el.GetAttribute("href").Trim(); //+ "&typ=sell&sort=lst.d&size=30&cur=BYR&rgn=4&ar=12";


                //if (linkCategory == linkMainCategory) continue;

                Send.InfoToLog(p, $"{nameCategory}");

                string query = String.Format(queryFormat, nameCategory, idCategory, linkCategory, name);

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