using SQLite;
using System;
using System.Diagnostics;

namespace Kufar.SQL
{
    public class Request
    {
        /// <summary>Создаем БД</summary>
        public void CreateBD()
        {

            using (var db = new SQLiteConnection(Settings.pathBD))
            {
                db.CreateTable<Ads>();
            }

        }

        /// <summary>Добавляем запись в БД</summary>
        public bool InsertBD(DataParse dataParse)
        {
            Ads ad = logDataAds(dataParse);

            int results = -1;
            var options = new SQLiteConnectionString(Settings.pathBD, false);
            using (var conn = new SQLiteConnection(options))
            {
                try
                {
                    results = conn.Insert(ad);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }

                return true;
            }


        }



        #region методы Получения значений из БД
        /// <summary>Получаем объявление из БД для парсинга телефона</summary>
        public Ads GetAdFromBDForParsePhone()
        {


            Ads results = null;
            var options = new SQLiteConnectionString(Settings.pathBD, false);
            using (var conn = new SQLiteConnection(options))
            {
                try
                {
                    results = conn.Table<Ads>().Where(a => a.status == Status.Null).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }


                if (results == null) return null;

                //обновляем запись
                results.status = Status.run;
                try
                {
                    conn.Update(results);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }

                return results;


            }


        }


        public Ads GetAdFromIdBD(int idBd)
        {
            Ads results = null;
            var options = new SQLiteConnectionString(Settings.pathBD, false);
            using (var conn = new SQLiteConnection(options))
            {
                try
                {
                    results = conn.Table<Ads>().Where(a => a.id == idBd).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
            return results;

        }

        /// <summary>Получаем данные из БД, где спаршен номер телефона</summary>
        public Ads GetAdIfParseNumberPhone()
        {
            Ads results = null;
            var options = new SQLiteConnectionString(Settings.pathBD, false);
            using (var conn = new SQLiteConnection(options))
            {
                try
                {
                    results = conn.Table<Ads>().Where(a => a.status == Status.parsePhoneNomer && a.phoneNumber != null).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }


                if (results == null) return null;

                //обновляем запись
                results.status = Status.run;
                try
                {
                    conn.Update(results);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }
            }
            return results;

            return null;
        }




        #endregion


        #region Обновление данных в БД
        public void UpdatePhoneNomberByIdBD(int idBd, string phoneNumber, Status status)
        {
            Ads results = null;
            var options = new SQLiteConnectionString(Settings.pathBD, false);
            using (var conn = new SQLiteConnection(options))
            {
                //получаем данные по ID
                try
                {

                    results = conn.Table<Ads>().Where(a => a.id == idBd).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
                }

                //Обновляем данные
                results.phoneNumber = phoneNumber;
                results.status = status;
                results.dateWrite = DateTime.Now;

                //Делаем запрос
                try
                {
                    conn.Update(results);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }
            }

        }


        public void UpdateFromProducts(DataParse dataParse, Ads ad, Status status = Status.parseFromKufar)
        {
            Ads results = null;
            var options = new SQLiteConnectionString(Settings.pathBD, false);
            using (var conn = new SQLiteConnection(options))
            {
                //получаем данные по ID
                try
                {

                    results = conn.Table<Ads>().Where(a => a.id == ad.id).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
                }

                Ads newAd = logDataAds(dataParse, ad);
                newAd.status = status;
                newAd.dateWrite = DateTime.Now;


                //Делаем запрос
                try
                {
                    conn.Update(newAd);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }
            }

        }



        #endregion





        public static Ads logDataAds(DataParse data)
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



            Ads ad = new Ads()
            {
                categoryName = data.CategoryName,

                condition = data.condition,
                updateDate = data.updateDate,

                descriptions = data.descriptions,
                idAd = data.Id,
                idCategory = data.Idcategory,
                idParentCategory = data.IdParentCategory,
                images = img,
                link = data.Link,
                oldAdParameters = parametrs,
                phoneNumber = data.phoneNumber,
                price = data.Price,
                title = data.Title,
                userName = data.UserName,

            };

            return ad;


        }

        public static Ads logDataAds(DataParse data, Ads ad)
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



            //обновляем только нужные данные
            ad.descriptions = data.descriptions.Trim();
            ad.images = img;
            ad.oldAdParameters = parametrs.Trim();



            return ad;


        }
    }
}
