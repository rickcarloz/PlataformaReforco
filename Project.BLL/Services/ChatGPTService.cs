using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Project.BLL.Services
{
    public class ChatGPTService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatGPTService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GerarRecomendacoesAsync(string tituloProva, double nota, int acertos, int totalQuestoes, List<RespostaAnalise> respostasErradas)
        {
            try
            {
                Console.WriteLine("=== DEBUG: ChatGPTService.GerarRecomendacoesAsync chamado ===");
                Console.WriteLine($"Título da prova: {tituloProva}");
                Console.WriteLine($"Nota: {nota}");
                Console.WriteLine($"Acertos: {acertos}/{totalQuestoes}");
                Console.WriteLine($"Respostas erradas: {respostasErradas?.Count ?? 0}");

                var apiKey = _configuration["ChatGPT:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("=== DEBUG: API Key não configurada, usando fallback ===");
                    return GerarRecomendacoesFallback(tituloProva, nota, acertos, totalQuestoes, respostasErradas);
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var prompt = ConstruirPrompt(tituloProva, nota, acertos, totalQuestoes, respostasErradas);
                Console.WriteLine($"=== DEBUG: Prompt construído: {prompt} ===");

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "Você é um tutor educacional especializado em fornecer feedback personalizado e recomendações de estudo." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("=== DEBUG: Enviando requisição para OpenAI ===");
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                
                Console.WriteLine($"=== DEBUG: Status da resposta: {response.StatusCode} ===");
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine("=== DEBUG: Rate limit atingido (429), usando fallback ===");
                    return GerarRecomendacoesFallback(tituloProva, nota, acertos, totalQuestoes, respostasErradas);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"=== DEBUG: Erro na API OpenAI: {response.StatusCode} - {errorContent} ===");
                    return GerarRecomendacoesFallback(tituloProva, nota, acertos, totalQuestoes, respostasErradas);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== DEBUG: Resposta da OpenAI: {responseContent} ===");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, options);
                var recomendacoes = result?.Choices?.FirstOrDefault()?.Message?.Content;


                if (string.IsNullOrEmpty(recomendacoes))
                {
                    Console.WriteLine("=== DEBUG: Resposta vazia da OpenAI, usando fallback ===");
                    return GerarRecomendacoesFallback(tituloProva, nota, acertos, totalQuestoes, respostasErradas);
                }

                Console.WriteLine($"=== DEBUG: Recomendações geradas com sucesso: {recomendacoes} ===");
                return recomendacoes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== DEBUG: Exception no ChatGPTService: {ex} ===");
                return GerarRecomendacoesFallback(tituloProva, nota, acertos, totalQuestoes, respostasErradas);
            }
        }

        private string ConstruirPrompt(string tituloProva, double nota, int acertos, int totalQuestoes, List<RespostaAnalise> respostasErradas)
        {
            var percentualAcertos = totalQuestoes > 0 ? (double)acertos / totalQuestoes * 100 : 0;
            var status = nota >= 7 ? "aprovado" : "reprovado";

            var prompt = $@"
Analise o desempenho do aluno na prova '{tituloProva}' e forneça recomendações personalizadas de estudo.

**Dados do Aluno:**
- Nota: {nota:F1}/10
- Acertos: {acertos}/{totalQuestoes} ({percentualAcertos:F1}%)
- Status: {status}

**Análise das Questões Erradas:**
";

            if (respostasErradas?.Any() == true)
            {
                prompt += "\nO aluno errou as seguintes questões:\n";
                foreach (var resposta in respostasErradas.Take(5))
                {
                    prompt += $"- Questão {resposta.NumeroQuestao}: {resposta.TextoQuestao}\n";
                    prompt += $"  Resposta escolhida: {resposta.RespostaEscolhida}\n";
                    prompt += $"  Resposta correta: {resposta.RespostaCorreta}\n\n";
                }
            }

            prompt += $@"

**Instruções:**
1. Forneça 3-4 recomendações específicas e práticas
2. Foque nos pontos onde o aluno teve dificuldade
3. Inclua sugestões de recursos de estudo
4. Seja motivacional e encorajador
5. Mantenha um tom profissional mas amigável

**Formato da resposta:**
Responda em português brasileiro, de forma clara e estruturada, com tópicos bem definidos.";

            return prompt;
        }

        private string GerarRecomendacoesFallback(string tituloProva, double nota, int acertos, int totalQuestoes, List<RespostaAnalise> respostasErradas)
        {
            Console.WriteLine("=== DEBUG: Gerando recomendações de fallback ===");
            
            var percentualAcertos = totalQuestoes > 0 ? (double)acertos / totalQuestoes * 100 : 0;
            
            var recomendacoes = new StringBuilder();
            recomendacoes.AppendLine($"📚 **Análise da Prova: {tituloProva}**");
            recomendacoes.AppendLine();
            recomendacoes.AppendLine($"🎯 **Seu Desempenho:**");
            recomendacoes.AppendLine($"• Nota: {nota:F1}/10");
            recomendacoes.AppendLine($"• Acertos: {acertos}/{totalQuestoes} ({percentualAcertos:F1}%)");
            recomendacoes.AppendLine();

            if (percentualAcertos >= 80)
            {
                recomendacoes.AppendLine("🌟 **Excelente trabalho!** Você demonstrou domínio do conteúdo.");
                recomendacoes.AppendLine("💡 **Sugestões para continuar:**");
                recomendacoes.AppendLine("• Revise os conceitos para consolidar o conhecimento");
                recomendacoes.AppendLine("• Explore tópicos mais avançados relacionados ao assunto");
                recomendacoes.AppendLine("• Ajude outros colegas que possam ter dificuldades");
            }
            else if (percentualAcertos >= 60)
            {
                recomendacoes.AppendLine("👍 **Bom trabalho!** Você tem uma base sólida no conteúdo.");
                recomendacoes.AppendLine("💡 **Sugestões para melhorar:**");
                recomendacoes.AppendLine("• Revise os tópicos onde você teve dificuldades");
                recomendacoes.AppendLine("• Pratique com exercícios similares");
                recomendacoes.AppendLine("• Consulte material adicional sobre os conceitos");
            }
            else
            {
                recomendacoes.AppendLine("📖 **Há espaço para melhorar!** Não desanime, vamos trabalhar juntos.");
                recomendacoes.AppendLine("💡 **Sugestões para melhorar:**");
                recomendacoes.AppendLine("• Revise o material didático com atenção");
                recomendacoes.AppendLine("• Faça anotações durante os estudos");
                recomendacoes.AppendLine("• Pratique com exercícios básicos primeiro");
                recomendacoes.AppendLine("• Considere formar grupos de estudo");
            }

            if (respostasErradas?.Any() == true)
            {
                recomendacoes.AppendLine();
                recomendacoes.AppendLine("❌ **Questões que precisam de atenção:**");
                foreach (var erro in respostasErradas.Take(3)) // Limitar a 3 para não ficar muito longo
                {
                    recomendacoes.AppendLine($"• Questão {erro.NumeroQuestao}: {erro.TextoQuestao}");
                    recomendacoes.AppendLine($"  Sua resposta: {erro.RespostaEscolhida}");
                    recomendacoes.AppendLine($"  Resposta correta: {erro.RespostaCorreta}");
                    recomendacoes.AppendLine();
                }
            }

            recomendacoes.AppendLine("🚀 **Dica:** Mantenha a consistência nos estudos e não hesite em pedir ajuda quando necessário!");

            var resultado = recomendacoes.ToString();
            Console.WriteLine($"=== DEBUG: Recomendações de fallback geradas: {resultado} ===");
            return resultado;
        }

        public class RespostaAnalise
        {
            public int NumeroQuestao { get; set; }
            public string TextoQuestao { get; set; } = string.Empty;
            public string RespostaEscolhida { get; set; } = string.Empty;
            public string RespostaCorreta { get; set; } = string.Empty;
        }

        public class OpenAIResponse
        {
            public List<Choice> Choices { get; set; } = new();
        }

        public class Choice
        {
            public Message Message { get; set; } = new();

        }

        public class Message
        {
            public string Content { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;    // Adicionado

        }
    }
} 