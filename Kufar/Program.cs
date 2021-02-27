using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode
    {
        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            int executionResult = 0;

            Send.InfoToLog(project, "Старт шаблона");
            string pathProfile = project.Directory + @"\profile\kufar_profile.zpprofile";

            // Выполнить группу действий ActionGroup001
            //executionResult = ActionGroup001.Execute(instance, project);

            //executionResult = Parser.Execute(instance, project, false);

            //string pathProfile = project.Directory + @"\profile\kufar_profile.zpprofile";
            string proxy = "45.89.231.240:55762:xHTsepsS:Tb9qBfym";
            string item = "119981167";


            string urlProduct = "https://auto.kufar.by/vi/120176604";
            urlProduct = "https://www.kufar.by/item/119981167";


            Send.InfoToLog(project, "Авторизация через профиль");
            Parser.AvtorizationByProfile(instance, project, pathProfile);

            Send.InfoToLog(project, "Получаем номер телефона");

            string phone = null;
            try
            {
                phone = Parser.GetNomerPhone(instance, project, item, proxy);
            }
            catch (Exception ex)
            {
                Send.InfoToLog(project, ex.Message);

            }



            //парсим недвижимость
            if (urlProduct.Contains("re.kufar.by"))
            {
                //гет запрос
                if (!Parser.ProductCardApartments(instance, project, urlProduct))
                {
                    //веб эмуляция
                    Parser.ProductCardApartments(instance, project, urlProduct, false);
                }
            }
            //парсим все остальное
            else
            {
                //гет запрос
                if (!Parser.ProductCard(instance, project, urlProduct))
                {   
                    //веб эмуляция
                    Parser.ProductCard(instance, project, urlProduct, false);
                }

            }





            

            //Send.InfoToLog(project, phone);


            if (executionResult != 0) return executionResult;

            return executionResult;
        }
    }
}