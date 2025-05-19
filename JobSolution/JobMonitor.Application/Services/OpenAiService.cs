using JobMonitor.Domain.Interfaces.Services;
using OpenAI.Chat;
using System.ClientModel;

namespace JobMonitor.Infrastructure.Services
{
    public class OpenAiService : IOpenAIService
    {
        private readonly ChatClient _chatClient;

        // Конструктор, передаем API ключ через переменную среды или конфигурацию
        public OpenAiService(string apiKey)
        {
            _chatClient = new ChatClient("gpt-4", apiKey);

        }

        // Метод для генерации текста в ответ на запрос
        public async Task<string> GenerateCoverLetterAsync(string jobTitle, string company, string skills)
        {
            var prompt = $"Напишите сопроводительное письмо для вакансии '{jobTitle}' в компании '{company}', с учетом навыков: {skills}. В письме должен быть приветственный тон, указание на опыт и желание работать в компании.";

            // Создание запроса
            ChatCompletion result = await _chatClient.CompleteChatAsync(prompt);

            // Извлекаем текст из первого объекта Choice
            var responseText = result.Content[0].Text;

            return responseText;
        }

        // Метод для поточной генерации текста
        public async Task StreamCoverLetterAsync(string jobTitle, string company, string skills)
        {
            var prompt = $"Напишите сопроводительное письмо для вакансии '{jobTitle}' в компании '{company}', с учетом навыков: {skills}. В письме должен быть приветственный тон, указание на опыт и желание работать в компании.";

            CollectionResult<StreamingChatCompletionUpdate> streamingCompletion = _chatClient.CompleteChatStreaming(prompt);

            // Обработка каждого полученного частичного ответа от сервера
            foreach (StreamingChatCompletionUpdate completionUpdate in streamingCompletion)
            {
                if (completionUpdate.ContentUpdate.Count > 0)
                {
                    Console.Write(completionUpdate.ContentUpdate[0].Text);
                }
            }
        }
    }
}
