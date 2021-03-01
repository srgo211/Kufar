using Kufar.SQL;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    internal class Parser
    {
        public const string connectBD = "Server=localhost;Database=kufar;Uid=root;Pwd=vertrigo;Port=3306;CharSet=utf8;Pooling=True;";
        public const string baseUrl = "https://www.kufar.by/listings?rgn=4&ar=12";
        public const int errAllCount = 20; //ВСЕГО ошибок подряд при добавлении в БД
        public const string rgxParseJson = "(?<=id=\"__NEXT_DATA__\" type=\"application/json\">)[\\w\\W]*?(?=</script>)";
        /// <summary>
        /// Метод выполнения группы действий
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <param name="checkGet">делаем Get запрос - true, Веб - false</param>
        /// <returns>Код выполнения группы действий</returns>
        public static void Execute(Instance instance, IZennoPosterProjectModel project, bool checkGet = true, int countParsePage = -1, string proxy = null)
        {
            //проверка на 0 страниц для парсинга
            if (countParsePage == 0) return;


            string url = baseUrl;
            string pathBD = project.Directory + @"\kufar.db";

            int correntPage = 1; //номер текущей страницы

            while (true)
            {
                string res = GetAndWebResult(instance, project,
                                            url: url,
                                            checkGet: checkGet,
                                            proxy: proxy);

                if (res == null)
                {
                    Send.InfoToLog(project, $"Пришел пустой ответ\n" +
                                            $"url: {url}\n" +
                                            $"Страница парсинга {correntPage}\n");
                    return;
                }

                //получаем json формат
                dynamic json = ResultJson(res);

                int correntAdPage = 0; //номер текущего объявления на странице
                int errAdFromBD = 0; //кол-во ошибок подряд, при добавлении в БД
                //Перебираем полученные данные
                foreach (var j in json?.props?.initialState?.listingGeneralist?.listingElements)
                {

                    //Формируем данные
                    DataParse data = new DataParse()
                    {
                        Id = j.id,
                        CategoryName = j.categoryName,
                        Idcategory = j.category,
                        IdParentCategory = j.parent,
                        Link = j.adViewLink,
                        Price = j.price.ru,
                        Title = j.title,
                        UserName = j.userName,
                        condition = j.adParameters?.condition?.vl,
                        updateDate = j.updateDate

                    };

                    Send.InfoToLog(project, $"Id - {data.Id}\n" +
                                            $"CategoryName - {data.CategoryName}\n" +
                                            $"Link - {data.Link}\n" +
                                            $"Price - {data.Price}\n" +
                                            $"Title  - {data.Title }+ \n");

                    //добавляем данные в БД
                    bool check = new Request().InsertBD(data);

                    #region проверка на ошибку при добавлении в БД
                    //проверка на ошибку при добавлении в БД
                    if (!check)
                    {
                        errAdFromBD++;
                    }

                    //Если кол-во ошибок подряд, при добавлении в БД, достигло макс, то выходим
                    //скорее всего добавляется уже  дубли
                    if (errAdFromBD >= errAllCount)
                    {
                        return;
                    }
                    #endregion


                    correntAdPage++; //номер объявления на текущей странице
                    Send.InfoToLog(project, $"Спарсили {correntAdPage} обявление на странице {correntPage}");
                }


                //проврка на кол-во страниц для парсинга
                if (countParsePage != -1)
                {
                    if (correntPage >= countParsePage)
                    {
                        Send.InfoToLog(project, $"Завершаем основной парсинг, прошли: {correntPage}");
                        return;
                    }

                }
                //прибавляем текущую страницу
                correntPage++;
                Send.InfoToLog(project, $"Спарсили: {correntPage} страниц");


                //токен для перехода на следующую страницу
                string next = string.Empty;
                //ищем токен
                foreach (var el in json?.props?.initialState?.listingGeneralist?.pagination)
                {
                    if (el.label == "next")
                    {
                        next = el.token;
                        break;
                    }
                }

                Send.InfoToLog(project, next);

                //проверка токена
                if (string.IsNullOrWhiteSpace(next)) return;

                //формируем новую ссылку, для перехода на следующую страницу
                url = $"{baseUrl}&cursor={next}";



            }//конец цикла
        }

        /// <summary>
        /// Авторизируемся в акк
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="project"></param>
        /// <param name="pathProfile"></param>
        public static void AvtorizationByProfile(Instance instance, IZennoPosterProjectModel project, string pathProfile)
        {
            project.Profile.Load(pathProfile);

            instance.ActiveTab.NavigateAndWait(baseUrl, 2000);

            //TODO проверка на авторизацию

        }


        /// <summary>
        /// Парсим карточку товара
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="project"></param>
        /// <param name="data">данные товара</param>
        /// <param name="checkGet">делаем Get запрос - true, Веб - false</param>
        /// <param name="proxy"></param>
        public static void ProductCard(Instance i, IZennoPosterProjectModel p, DataParse data, bool checkGet = true, string proxy = null)
        {
            string url = data.Link;

            if (string.IsNullOrWhiteSpace(url)) return;

            string res = GetAndWebResult(i, p, url, checkGet, proxy);

            if (string.IsNullOrWhiteSpace(res)) return;

            dynamic json = ResultJson(res);

            if (json == null) return;

            Dictionary<string, string> oldAdParameters = new Dictionary<string, string>();

            dynamic j = json?.props?.initialState?.adview;

            if (j == null) return;

            foreach (var parameter in j.result?.ad_parameters)
            {
                oldAdParameters.Add(parameter.pl, parameter.vl);
            }

            //получаем доп. описание (тех. харк-ки)
            data.oldAdParameters = oldAdParameters;

            //получаем фото
            data.images = j.formattedAdView?.gallery?.images;

            //получае описание товара/услуги
            data.descriptions = j.formattedAdView?.descriptions;

        }

        public static DataParse ProductCard(Instance i, IZennoPosterProjectModel p, string url, bool checkGet = true, string proxy = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            string res = GetAndWebResult(i, p, url, checkGet, proxy);

            if (string.IsNullOrWhiteSpace(res)) return null;

            dynamic json = ResultJson(res);

            if (json == null) return null;

            DataParse data = new DataParse();
            Dictionary<string, string> oldAdParameters = new Dictionary<string, string>();

            dynamic j = json?.props?.initialState?.adview;

            //initialState.adView

            if (j == null)
            {
                j = json?.props?.initialState?.adView;
                if (j == null) return null;


                return parseJson(p, json);
            }


            var result = j.result;
            if (result == null) return null;


            foreach (var parameter in result.ad_parameters)
            {
                string key = parameter.pl;
                string value = parameter.vl;
                oldAdParameters.Add(key, value);
            }



            data.Id = result.ad_id;
            data.Link = result.ad_link;
            data.Idcategory = result.category;
            data.updateDate = result.list_time;
            data.UserName = result.name;
            data.descriptions = result.body;
            data.oldAdParameters = oldAdParameters; //получаем доп. описание (тех. харк-ки)
            data.Price = result.price_byn; //цена (полный формат - в копейках)


            var formattedAdView = j.formattedAdView;
            if (formattedAdView == null) return null;
            data.Title = formattedAdView.subject;
            data.CategoryName = formattedAdView.categoryName;


            var arrImages = formattedAdView.gallery?.images;
            var arr = arrImages.ToObject<string[]>();

            data.images = arr;



            return data;
        }


        public static DataParse ProductCardApartments(Instance i, IZennoPosterProjectModel p, string url, bool checkGet = true, string proxy = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            string res = GetAndWebResult(i, p, url, checkGet, proxy);

            if (string.IsNullOrWhiteSpace(res)) return null;

            dynamic json = ResultJson(res);

            if (json == null) return null;

            DataParse data = new DataParse();
            Dictionary<string, string> oldAdParameters = new Dictionary<string, string>();

            dynamic j = json?.props?.initialState?.adView?.data;


            foreach (var parameter in j.oldAdParameters)
            {
                string key = parameter.pl;

                switch (key.ToLower())
                {
                    case "координаты":
                        continue;
                    default:
                        break;
                }


                string value = String.Empty;
                try
                {
                    value = parameter.vl;
                }
                catch { continue; }

                oldAdParameters.Add(key, value);
            }





            data.Id = j.id;
            data.Link = j.adViewFriendlyLink;
            data.Idcategory = j.category;
            data.CategoryName = j.categoryName;
            data.Price = j.price.by; //цена (полный формат - в копейках)
            data.updateDate = j.updateDate;
            data.descriptions = j.descriptions;
            data.oldAdParameters = oldAdParameters; //получаем доп. описание (тех. харк-ки)
            data.Title = j.subject;


            //data.UserName = result.name;





            var arrImages = j.allImages;
            var arr = arrImages.ToObject<string[]>();
            data.images = arr;





            logData(p, data);

            return data;
        }

        /// <summary>
        /// Получаем номер телефона (АКК должен быть авторизирован, только белорусские IP!!!)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="project"></param>
        /// <param name="idItem">id объявления</param>
        /// <param name="proxy">прокси в фомате (ip:port:login:pass)</param>
        /// <returns></returns>
        public static string GetNomerPhone(Instance instance, IZennoPosterProjectModel project, string idItem, string proxy = null)
        {
            //Получаем юзер агент
            string userAgent = project.Profile.UserAgent;

            //получаем ссылку для парсинга номера телефона
            string url = $"https://cre-api.kufar.by/items-search/v1/engine/v1/item/{idItem}/phone";
            string res = string.Empty;





            //Получаем куки из ЗЕНКИ и передаем в XNET
            CookieStorage cookieStorage = GetCookie(instance, project);

            res = GetXnet(proxy, userAgent, url, cookieStorage);

            dynamic json = JObject.Parse(res);

            //Send.InfoToLog(project, res);
            return json?.phone;
        }

        /// <summary>
        /// Запрос с помощью Xnet
        /// </summary>
        /// <param name="proxy">прокси</param>
        /// <param name="userAgent">юзер агент</param>
        /// <param name="url">url адрес</param>
        /// <param name="cookieStorage">куки</param>
        /// <returns></returns>
        private static string GetXnet(string proxy, string userAgent, string url, CookieStorage cookieStorage)
        {


            string res;
            using (var rq = new HttpRequest())
            {
                rq.UserAgent = userAgent;

                rq.UseCookies = true;

                rq.Cookies = cookieStorage;

                //rq.Cookies.IsLocked = true; куки могут меняться автоматически, чтоб этого не произшло - true

                if (!string.IsNullOrWhiteSpace(proxy)) rq.Proxy = ProxyClient.Parse(ProxyType.HTTP, proxy);


                // Отправляем запрос.
                HttpResponse response = rq.Get(url);

                // Принимаем тело сообщения в виде строки.
                res = response.ToString();


            }

            return res;
        }


        /// <summary>Получаем куки из зенки и передаем в XNET</summary>
        private static CookieStorage GetCookie(Instance instance, IZennoPosterProjectModel project)
        {
            #region Способ - 1 Куки берем из cookieContainer
            /*
            ICookieContainer cookieContainer = project.Profile.CookieContainer;

            IEnumerable<string> domains = cookieContainer.Domains;

            //иницилизация кук
            CookieStorage cookieStorage = new CookieStorage();
            foreach (var domen in domains)
            {
                //получаем куки с инстанса (акк должен быть авторизирован)
                string cookies = instance.GetCookie(domen, true);


                if (cookies.Contains("!bidswitch"))
                {
                    Send.InfoToLog(project, "");
                }


                //получаем массив кук из инстанса
                string[] arrCook = cookies.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);

                //добавляем куки
                foreach (var el in arrCook)
                {
                    if (el.Contains("=="))
                    {
                        string c = el.Replace("==", "||");

                        var Arr = c.Split('=');
                        Cookie cooki = new Cookie(Arr[0].Trim(), Arr[1].Trim().Replace("||", "=="));
                        cooki.Domain = domen;
                        cookieStorage.Add(cooki);
                        continue;

                    }




                    string[] elArr = el.Split('=');

                    string key = elArr[0].Trim();
                    string value = elArr[1].Trim();

                    if (value.Contains("!bidswitch")) continue;

                    var cook = new Cookie(key, value);
                    cook.Domain = domen;
                    cookieStorage.Add(cook);
                }

            }
            */
            #endregion

            //иницилизация кук
            CookieStorage cookieStorage = new CookieStorage();

            ICookieContainer cookieContainer = project.Profile.CookieContainer;
            IEnumerable<string> domains = cookieContainer.Domains;

            foreach (var domen in domains)
            {

                // get cookie items
                var items = project.Profile.CookieContainer.Get(domen);
                // output to log info of cookie item
                foreach (var item in items)
                {
                    Send.InfoToLog(project, item.Name + " = " + item.Value);

                    var cook = new Cookie(item.Name, item.Value);
                    cook.Domain = domen;
                    cookieStorage.Add(cook);

                }




            }


            return cookieStorage;
        }

        public static string GetNomerPhoneWeb(Instance instance, IZennoPosterProjectModel project, string url, string proxy = null)
        {
            instance.LoadPictures = false; //картинки
            instance.UseAdds = false; //реклама
            instance.UseMedia = false; //видео/аудио
            //кнопка позвонить
            const string xpBtnCall = "//button[@data-name = 'call_button']";

            const string xpPhone = "//div[@data-name='phone-number-modal']/a";

            if (string.IsNullOrWhiteSpace(url)) return null;

            string res = string.Empty;

            if (!string.IsNullOrWhiteSpace(proxy))
            {
                proxy = ConvertProxy(proxy);

                instance.SetProxy(proxy,
                    useProxifier: true,
                    emulateGeolocation: true,
                    emulateTimezone: true,
                    emulateWebrtc: true
                    );
            }


            Tab tab = instance.ActiveTab;

            try
            {
                tab.NavigateAndWait(url, 500).WaitElement(xpBtnCall, 2000, exceptionMessage: "Не нашли кнопку позвонить");
            }
            catch
            {
                return null;
            }

            if (tab.Click(xpBtnCall, 500).CheckHtmlElement(xpPhone, 3000))
            {
                return tab.FindElementByXPath(xpPhone, 0).InnerText.Trim();
            }


            return null;



        }

        #region Вспомогательные методы
        /// <summary>
        /// Делаем запрос
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="project"></param>
        /// <param name="checkGet">делаем Get запрос - true, Веб - false</param>
        /// <param name="proxy">прокси</param>
        /// <param name="url">url для запроса</param>
        /// <returns>результат запроса</returns>
        static string GetAndWebResult(Instance instance, IZennoPosterProjectModel project,
                                        string url,
                                        bool checkGet = true,
                                        string proxy = null)
        {
            string userAgent = project.Profile.UserAgent;
            string res = string.Empty;

            //ПРоверка способа парсинга (Веб или запросы)
            if (checkGet)
            {
                using (var request = new HttpRequest())
                {
                    //присваиваем юзер агент
                    request.UserAgent = userAgent;

                    //проверка на прокси
                    if (!string.IsNullOrWhiteSpace(proxy)) request.Proxy = ProxyClient.Parse(ProxyType.HTTP, proxy);

                    // Отправляем запрос.
                    HttpResponse response = request.Get(url);

                    // Принимаем тело сообщения в виде строки.
                    res = response.ToString();


                }
            }
            else
            {
                //проверка на прокси
                if (!string.IsNullOrWhiteSpace(proxy)) instance.SetProxy(proxy,
                                                        useProxifier: true,
                                                        emulateGeolocation: true,
                                                        emulateTimezone: true,
                                                        emulateWebrtc: true);

                instance.ActiveTab.NavigateAndWait(url, 500);

                res = instance.ActiveTab.DomText;


            }

            return res;
        }


        /// <summary>
        /// Получаем результат в JSON формате
        /// </summary>
        /// <param name="res">текст для парсинга</param>
        /// <param name="regex">регулярка</param>
        /// <returns></returns>
        static dynamic ResultJson(string res, string regex = rgxParseJson)
        {
            //выдергиваем регуляркой данные
            string jsonResult = new Regex(regex).Match(res).ToString();

            //проверка на пусттоту
            if (string.IsNullOrEmpty(jsonResult)) return null;

            //получаем json формат
            dynamic json = null;
            try
            {
                json = JObject.Parse(jsonResult);
            }
            catch (Exception ex)
            {
                return null;
            }

            return json;
        }



        static DataParse parseJson(IZennoPosterProjectModel p, dynamic json)
        {


            if (json == null) return null;

            DataParse data = new DataParse();
            Dictionary<string, string> oldAdParameters = new Dictionary<string, string>();

            dynamic j = json?.props?.initialState?.adView?.data;

            var oldAdParameter = j.oldAdParameters ?? j.initial?.ad_parameters;
            if (oldAdParameter != null)
            {
                foreach (var parameter in oldAdParameter)
                {
                    string key = parameter.pl;

                    switch (key.ToLower())
                    {
                        case "координаты":
                        case "товары с куфар доставкой":
                        case "товары с куфар оплатой":
                        case "тип сделки":
                        case "тип оплаты":
                            continue;

                        case "состояние":
                            data.condition = parameter.vl;
                            continue;

                        default:
                            break;
                    }


                    string value = String.Empty;
                    try
                    {
                        value = parameter.vl;
                    }
                    catch { continue; }

                    oldAdParameters.Add(key, value);
                }
            }




            data.Id = j.id;
            data.Link = j.adViewFriendlyLink ?? j.adViewLink;
            data.Idcategory = j.category;
            data.CategoryName = j.categoryName;
            data.Price = j.price.by; //цена (полный формат - в копейках)
            data.updateDate = j.updateDate;
            data.descriptions = j.descriptions;
            data.oldAdParameters = oldAdParameters; //получаем доп. описание (тех. харк-ки)
            data.Title = j.subject ?? j.title;


            data.UserName = j.name ?? j.userName;




            //получаем фото
            var arrImages = j.allImages ?? j.gallery?.images;
            if (arrImages != null)
            {
                var arr = arrImages.ToObject<string[]>();
                data.images = arr;
            }




            logData(p, data);
            return data;
        }




        static string ConvertProxy(string proxy, bool formatZenno = true)
        {

            if (proxy.Contains("@") || !formatZenno) return proxy;

            var arrProxy = proxy.Trim().Split(':');
            string ip = arrProxy[0];
            string port = arrProxy[1];
            string login = arrProxy[2];
            string password = arrProxy[3];

            string newProxy = $"{login}:{password}@{ip}:{port}";
            return newProxy;

        }


        public static void logData(IZennoPosterProjectModel p, DataParse data)
        {


            const int symbol = 30; //кол-во символов (....)
            const string charSymbol = "."; //кол-во символов (....)
            string parametrs = string.Empty;

            if (data.oldAdParameters != null)
            {
                foreach (var el in data.oldAdParameters)
                {
                    int keyLength = el.Key.Length;

                    int countPresent = symbol - keyLength;
                    string symb = String.Empty;
                    for (int a = 0; a < countPresent; a++) symb += charSymbol;

                    parametrs += $"{el.Key}{symb}{el.Value}\n";
                }
            }


            string img = string.Empty;
            if (data.images != null)
            {
                img = String.Join("\n", data.images);
            }

            string log =
                 $"Id- {data.Id}\n" +
                 $"Link- {data.Link}\n" +
                $"Категория- {data.CategoryName}\n" +
                 $"Название- {data.Title}\n" +
                 $"Цена товара- {data.Price}\n" +
                 $"Состояние товара- {data.condition}\n" +
                 $"Хар-ки товара- \n{parametrs}\n" +
                 $"Описание товара- {data.descriptions}\n" +
                 $"ID категории- {data.Idcategory}\n" +
                 $"ID ПОДкатегории- {data.IdParentCategory}\n" +
                 $"Ссылки на фото - {img}\n" +
                 $"Номер телефона- {data.phoneNumber}\n" +
                 $"Дата публикации- {data.updateDate}\n" +
                 $"Имя пользователя- {data.UserName}\n";

            Send.InfoToLog(p, log);

        }

        #endregion
    }
}
