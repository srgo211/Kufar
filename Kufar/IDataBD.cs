using Kufar.SQL;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    interface IDataBD
    {
        /// <summary>
        /// Создание БД
        /// </summary>
        /// <param name="pathBD"></param>
        void CreateBD(IZennoPosterProjectModel project);

        void AddBD(DataParse dataParse);

        /// <summary>
        /// Получаем объявлеие для парсинга телефона и обновляем статус
        /// </summary>
        /// <param name="pathBD"></param>
        /// <param name="dataParse"></param>
        /// <returns></returns>
        Ads GetAdForParsePhoneBD();

        /// <summary>Получаем данные из БД по ID </summary>
        Ads GetAdFromIdBD(int idFromBD);

        /// <summary>Получаем данные из БД, где спаршен номер телефона</summary>
        Ads GetAdIfParseNumberPhone();



        /// <summary>
        /// Обновляем Номер телефона
        /// </summary>
        /// <param name="idBd">ид БД</param>
        /// <param name="phoneNumber">номер телефона</param>
        void UpdatePhoneNomberByIdBD(int idBd, string phoneNumber, Status status);


        void UpdateFromProducts(DataParse dataParse, Ads ad, Status status = Status.parseFromKufar);


        void DeleteBD(int idBD);

    }






}
