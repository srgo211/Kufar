using Kufar.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{

    public enum Status
    {
        /// <summary>В работе</summary>
        run,
        /// <summary>Выполнен успешно </summary>
        ok,
        /// <summary>Ошибка выполнения </summary>
        err,
        /// <summary>Остановлен </summary>
        stop,

        /// <summary>Опубликован в region</summary>
        publih,
        /// <summary>Спарсили с куфар</summary>
        parse
    }

    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode, IWorkingProcedure, IDataBD
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


            string pathBD = project.Directory + @"\kufar.db";
            //создаем БД
            CreateBD(project, pathBD);


            //получаем список всех объявлений
            List<DataParse> data = GetListDataParses(instance, project);

            return 0;
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


        #region Реализация интерфейса 
        /// <summary> Парсим все объявления </summary>

        public List<DataParse> GetListDataParses(Instance instance, IZennoPosterProjectModel project)
        {
            return Parser.Execute(instance, project, countParsePage: 1).Result;
        }

        /// <summary> Авторизация акк </summary>
        public void AvtorizationAccount(Instance instance, IZennoPosterProjectModel project)
        {
            string pathProfile = project.Directory + @"\profile\kufar_profile.zpprofile";

            Send.InfoToLog(project, "Авторизация через профиль");
            Parser.AvtorizationByProfile(instance, project, pathProfile);
        }


        /// <summary> Парсим номер телефона из объявлений </summary>
        public void GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, int IdFromBD, string proxy = null)
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
                                   $"{ex.Message}\n"
                                   //$"{ex.StackTrace}\n" +
                                   //$"{ex.InnerException}\n" +
                                   //$"{ex.Source}\n" +
                                   //$"{ex.Data}\n"
                                   );

                if (ex.Message.Contains("429"))
                {
                    System.Threading.Thread.Sleep(2000);
                    phone = Parser.GetNomerPhone(instance, project, data.Id.ToString(), proxy);
                    if (string.IsNullOrEmpty(phone)) continue;
                    data.phoneNomer = phone;

                }
                if (ex.Message.Contains("400"))
                {
                    Send.InfoToLog(project, "нет кнопки позвонить");
                    continue;
                }

                if (ex.Message.Contains("401"))
                {
                    //должен быть номер
                    Send.InfoToLog(project, "не авторизован, слетела авторизации");


                    //получаем номер с помощью ВЕБ версии
                    phone = Parser.GetNomerPhoneWeb(instance, project, data.Link, proxy);
                    Send.InfoToLog(project, phone);


                    //получаем номер с помощью ВЕБ версии

                    if (string.IsNullOrEmpty(phone)) continue;
                    data.phoneNomer = phone;
                }






            }
            newDataParse.Add(data);


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
                // Parser.logData(project, parse);

                newDataParse.Add(parse);
            }

            return newDataParse;
        }
        #endregion


        #region Реализация интерфейса
        /// <summary> Добавляем данные в БД </summary>
        public void AddBD(List<DataParse> dataParses)
        {
            throw new NotImplementedException();
        }

        public void CreateBD(IZennoPosterProjectModel project, string pathBD)
        {

            //проверка есть ли БД по этому пути
            if (!File.Exists(pathBD))
            {
                //создаем БД
                new Request().CreateBD(pathBD);
                Send.InfoToLog(project, pathBD);
            }


        }

        public void AddBD(string pathBD, DataParse dataParse)
        {
            throw new NotImplementedException();
        }

        public Ads GetAdAndUpdateCheckPhoneBD(string pathBD, DataParse dataParse)
        {
            return new Request().GetAdFromBDForParsePhone(pathBD, dataParse);
        }

        public void UpdateBD(string pathBD, int idBD)
        {
            throw new NotImplementedException();
        }

        public void DeleteBD(string pathBD, int idBD)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}