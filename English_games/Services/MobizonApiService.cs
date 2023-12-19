using System.Text;
using System.Text.Json;
using English_games.Context;
using English_games.Models;
using English_games.ViewModels.Json;
using Microsoft.EntityFrameworkCore;

namespace English_games.Services;

public class MobizonApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://api.mobizon.kz";
    private readonly English_gamesContext _db;

    public MobizonApiService(HttpClient httpClient, English_gamesContext db)
    {
        _httpClient = httpClient;
        _db = db;
    }

    public async Task SendMessageAsync(string recipient, string text)
    {
        string apiUrl = $"{_apiBaseUrl}/service/Message/SendSmsMessage";
        string apiKey = "kz04d8ee03c42dd8c4bf53ea7d0ac79d6b058a54aecc97bee4a268909fd8af2b6d9098";
        var requestData = new MultipartFormDataContent
        {
            { new StringContent(recipient), "recipient" },
            { new StringContent(text), "text" },
            { new StringContent(apiKey), "apiKey" },
            { new StringContent("1440"), "params[validity]" }
        };

        var response = await _httpClient.PostAsync(apiUrl, requestData);

        if (response.IsSuccessStatusCode)
        {
            // Сообщение успешно отправлено
            var responseContent = await response.Content.ReadAsStringAsync();
            // Обработайте responseContent по вашему усмотрению
        }
        else
        {
            // Произошла ошибка при отправке сообщения
            string errorMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка: {response.StatusCode} - {errorMessage}");
        }
    }

    public async Task BalanceInMobizon()
    {
        string apiKey = "kz04d8ee03c42dd8c4bf53ea7d0ac79d6b058a54aecc97bee4a268909fd8af2b6d9098";
        string apiUrl = "http://api.mobizon.kz/service/user/getOwnBalance";
    
        string getParams = $"output=json&api=v1&apiKey={apiKey}";
        string fullUrl = $"{apiUrl}?{getParams}";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Баланс на счете: " + responseData);
                var responseObject = JsonSerializer.Deserialize<ResponseObject>(responseData);
                if (responseObject != null && responseObject.data != null)
                {
                    var mobizon = await _db.Mobizons.FirstOrDefaultAsync(m => m.Id == 1);
                    if (mobizon == null)
                    {
                        // Если объект не существует, создайте новый
                        mobizon = new Mobizon { Id = 1, Balance = "0" };
                        _db.Mobizons.Add(mobizon);
                        await _db.SaveChangesAsync();
                    }

                    string balance = responseObject.data.balance;
                    mobizon.Balance = balance;
                    _db.Mobizons.Update(mobizon);
                    await _db.SaveChangesAsync();
                    Console.WriteLine("Баланс на счете: " + balance.GetType());
                }
            }
            else
            {
                Console.WriteLine("Error occurred while fetching user balance. Status code: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }


  


    
    // Отправка SMS с использованием Mobizon API Get
    // var mobizonApiKey = "kzc1e36affd5e700cd1d110bfa89244dcce64be0dbc55fb447d9e3c2a90b984c1b2cd4";
    // var recipient = user.PhoneNumber; // Замените это на поле с номером телефона пользователя
    // var text = $"Код для InterHouse_kz {codeNumber}"; // Используйте сгенерированный код
    // var mobizonUrl = $"http://api.mobizon.kz/service/message/sendsmsmessage?recipient={recipient}&text={text}&apiKey={mobizonApiKey}";
    //
    // using (HttpClient client = new HttpClient())
    // {
    //     var response = await client.GetStringAsync(mobizonUrl);
    //     Console.WriteLine(response);
    // }
}