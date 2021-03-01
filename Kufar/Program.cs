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
        Null,

        /// <summary>В работе</summary>
        run,


        /// <summary>Парсим номер телефона</summary>
        parsePhoneNomer,

        /// <summary>Спарсили полную карточку товара + телефон</summary>
        parseFromKufar,

        /// <summary>Опубликован в region</summary>
        publih,

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

            Settings.pathBD = project.Directory + @"\kufar.db";


            //создаем БД
            CreateBD(project);


            //получаем список всех объявлений и заносим в БД
            //GetListDataParses(instance, project, -1);

            // return 0;
            //делаем авторизацию, для сбора номеров телефона
            AvtorizationAccount(instance, project);

            //Парсим номер телефона
            GetDataAndNomerPhone(instance, project, proxy);



            //получаем полную карточку по каждому товару и заносим в БД
            GetParseDataFromProducts(instance, project);










            if (executionResult != 0) return executionResult;

            return executionResult;
        }


        #region Реализация интерфейса 
        /// <summary> Парсим все объявления </summary>

        public void GetListDataParses(Instance instance, IZennoPosterProjectModel project, int counParsePage)
        {
            Parser.Execute(instance, project, countParsePage: counParsePage);
        }

        /// <summary> Авторизация акк </summary>
        public void AvtorizationAccount(Instance instance, IZennoPosterProjectModel project)
        {
            string pathProfile = project.Directory + @"\profile\kufar_profile.zpprofile";

            Send.InfoToLog(project, "Авторизация через профиль");
            Parser.AvtorizationByProfile(instance, project, pathProfile);
        }


        /// <summary> Парсим номер телефона из объявлений </summary>
        public void GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, string proxy = null)
        {
            while (true)
            {
                //Получаем объявление из БД для парсинга телефона
                Ads data = GetAdForParsePhoneBD();

                if (data == null) return;


                string phone = null;
                try
                {
                    //парсим номер телефона GET запросом
                    phone = Parser.GetNomerPhone(instance, project, data.idAd.ToString(), proxy);
                    data.phoneNumber = phone;
                    UpdatePhoneNomberByIdBD(data.id, phone, Status.parsePhoneNomer);
                }
                catch (Exception ex)
                {


                    Send.InfoToLog(project, $"{data.link}\n" +
                                       $"{ex.Message}"
                                       //$"{ex.StackTrace}\n" +
                                       //$"{ex.InnerException}\n" +
                                       //$"{ex.Source}\n" +
                                       //$"{ex.Data}\n"
                                       );

                    if (ex.Message.Contains("429"))
                    {
                        System.Threading.Thread.Sleep(2000);
                        phone = Parser.GetNomerPhone(instance, project, data.idAd.ToString(), proxy);
                        UpdatePhoneNomberByIdBD(data.id, phone, Status.parsePhoneNomer);
                        if (string.IsNullOrEmpty(phone)) continue;
                        data.phoneNumber = phone;
                        UpdatePhoneNomberByIdBD(data.id, phone, Status.parsePhoneNomer);
                    }
                    if (ex.Message.Contains("400"))
                    {
                        Send.InfoToLog(project, "нет кнопки позвонить");
                        UpdatePhoneNomberByIdBD(data.id, phone, Status.parsePhoneNomer);
                        continue;
                    }

                    if (ex.Message.Contains("401"))
                    {
                        //должен быть номер
                        Send.InfoToLog(project, "не авторизован, слетела авторизации");


                        //получаем номер с помощью ВЕБ версии
                        phone = Parser.GetNomerPhoneWeb(instance, project, data.link, proxy);
                        Send.InfoToLog(project, phone);


                        //получаем номер с помощью ВЕБ версии

                        if (string.IsNullOrEmpty(phone)) continue;
                        data.phoneNumber = phone;

                        UpdatePhoneNomberByIdBD(data.id, phone, Status.parsePhoneNomer);

                    }

                }





            }



        }

        /// <summary> Получаем полную карточку товара </summary>
        public void GetParseDataFromProducts(Instance instance, IZennoPosterProjectModel project)
        {
            Ads dataAd = null;

            while (true)
            {
                //Получаем объявление, где есть номер телефона
                dataAd = GetAdIfParseNumberPhone();

                if (dataAd == null) return;

                DataParse parse = null;
                string urlProduct = dataAd.link;

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


                //обновляем данные в БД
                UpdateFromProducts(parse, dataAd, Status.parseFromKufar);



            }


        }



        List<DataParse> IWorkingProcedure.GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, List<DataParse> dataParses, string proxy)
        {
            throw new NotImplementedException();
        }

        List<DataParse> IWorkingProcedure.GetDataAndNomerPhone(Instance instance, IZennoPosterProjectModel project, int IdFromBD, string proxy)
        {
            throw new NotImplementedException();
        }



        #endregion


        #region Реализация интерфейса
        /// <summary> Добавляем данные в БД </summary>
        public void AddBD(List<DataParse> dataParses)
        {
            throw new NotImplementedException();
        }

        public void CreateBD(IZennoPosterProjectModel project)
        {

            //проверка есть ли БД по этому пути
            if (!File.Exists(Settings.pathBD))
            {
                //создаем БД
                new Request().CreateBD();
                Send.InfoToLog(project, "Создали БД: " + Settings.pathBD);
            }


        }

        public void AddBD(DataParse dataParse)
        {
            throw new NotImplementedException();
        }



        /// <summary>Получаем объявление из БД для парсинга телефона</summary>
        public Ads GetAdForParsePhoneBD()
        {
            return new Request().GetAdFromBDForParsePhone();
        }

        Ads IDataBD.GetAdFromIdBD(int idFromBD)
        {
            throw new NotImplementedException();
        }

        public void UpdatePhoneNomberByIdBD(int idBD, string phoneNumber, Status status)
        {
            new Request().UpdatePhoneNomberByIdBD(idBD, phoneNumber, status);
        }

        void IDataBD.DeleteBD(int idBD)
        {
            throw new NotImplementedException();
        }

        /// <summary>Получаем объявление, где есть номер телефона</summary>

        public Ads GetAdIfParseNumberPhone()
        {
            return new Request().GetAdIfParseNumberPhone();
        }

        public void UpdateFromProducts(DataParse dataParse, Ads ad, Status status)
        {
            new Request().UpdateFromProducts(dataParse, ad, status);
        }















        #endregion
    }
}