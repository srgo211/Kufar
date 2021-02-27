using System;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode, IWorkingProcedure
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
            string proxy = "45.89.231.240:55762:xHTsepsS:Tb9qBfym";

            //получаем список всех объявлений
            var data = GetListDataParses(instance, project);

            //делаем авторизацию, для сбора номеров телефона
            AvtorizationAccount(instance, project);

            //выдергиваем все объявления, где есть номер телефона
            data = GetDataAndNomerPhone(instance, project, data, proxy);


            //получаем полную карточку по каждому товару
            data = GetParseDataFromProducts(instance, project, data);


            //добавляем данные в БД




            string urlProduct = "https://auto.kufar.by/vi/120176604";
            urlProduct = "https://www.kufar.by/item/119981167";





            if (executionResult != 0) return executionResult;

            return executionResult;
        }

        /// <summary> Парсим все объявления </summary>

        public List<DataParse> GetListDataParses(Instance instance, IZennoPosterProjectModel project)
        {
            return Parser.Execute(instance, project, countParsePage: 1);
        }

        /// <summary> Авторизация акк </summary>
        public void AvtorizationAccount(Instance instance, IZennoPosterProjectModel project)
        {
            string pathProfile = project.Directory + @"\profile\kufar_profile.zpprofile";

            Send.InfoToLog(project, "Авторизация через профиль");
            Parser.AvtorizationByProfile(instance, project, pathProfile);
        }


        /// <summary> Парсим номер телефона из объявлений </summary>
        public List<DataParse> GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, List<DataParse> dataParses, string proxy = null)
        {
            List<DataParse> newDataParse = new List<DataParse>();
            foreach (var data in dataParses)
            {

                string phone = null;
                try
                {
                    phone = Parser.GetNomerPhone(instance, project, data.Id.ToString(), proxy);
                    data.phoneNomer = phone;
                }
                catch (Exception ex)
                {
                    Send.InfoToLog(project, $"{data.Link}\n" +
                                            $"{ex.Message}\n" +
                                            $"{ex.StackTrace}\n" +
                                            $"{ex.InnerException}\n" +
                                            $"{ex.Source}\n" +
                                            $"{ex.Data}\n");

                    if (ex.Message.Contains("429")) System.Threading.Thread.Sleep(2000);
                    if (ex.Message.Contains("400")) Send.InfoToLog(project, "нет кнопки позвонить");

                    if (ex.Message.Contains("401"))
                    {
                        //должен быть номер
                        Send.InfoToLog(project, "не авторизован, слетела авторизации");

                        //string ip, int port, string type, string login = null, string password = null,
                        //xHTsepsS: Tb9qBfym@45.89.231.240:55762

                        proxy = $"{login}:{password}@{ip}:{port}";

                        //получаем номер с помощью ВЕБ версии
                        phone = Parser.GetNomerPhoneWeb(instance, project, data.Id.ToString(), proxy);
                        if (string.IsNullOrEmpty(phone)) continue;
                        data.phoneNomer = phone;
                    }
                }
                newDataParse.Add(data);
            }

            return newDataParse;


        }

        /// <summary> Получаем полную карточку товара </summary>
        public List<DataParse> GetParseDataFromProducts(Instance instance, IZennoPosterProjectModel project, List<DataParse> dataParses)
        {
            List<DataParse> newDataParse = new List<DataParse>();

            foreach (var data in dataParses)
            {
                DataParse parse = null;
                string urlProduct = data.Link;

                //парсим недвижимость
                if (urlProduct.Contains("re.kufar.by"))
                {
                    parse = Parser.ProductCardApartments(instance, project, urlProduct);

                    //гет запрос
                    if (parse == null)
                    {
                        //веб эмуляция
                        parse = Parser.ProductCardApartments(instance, project, urlProduct, false);
                    }
                }
                //парсим все остальное
                else
                {
                    parse = Parser.ProductCard(instance, project, urlProduct);
                    //гет запрос
                    if (parse == null)
                    {
                        //веб эмуляция
                        parse = Parser.ProductCard(instance, project, urlProduct, false);
                    }

                }

                if (parse == null) continue;

                parse.phoneNomer = data.phoneNomer;
                newDataParse.Add(parse);
            }

            return newDataParse;
        }


        /// <summary> Добавляем данные в БД </summary>
        public void AddBD(List<DataParse> dataParses)
        {
            throw new NotImplementedException();
        }
    }
}