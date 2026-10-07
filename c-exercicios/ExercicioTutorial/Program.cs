using System.Globalization;
using System.Text;

namespace ExercicioTutorial
{
    internal class Program
    {
        // Cultura brasileira para formatar valores como R$ 1.234,56
        static readonly CultureInfo ptBR = new CultureInfo("pt-BR");

        // Constantes de regras de negócio
        const decimal DESCONTO_VIP = 5m;      // 5% extra para clientes VIP
        const string CODIGO_PROMO = "PROMO10"; // código promocional válido
        const decimal DESCONTO_PROMO = 10m;   // 10% de desconto com o cupom
        const int MAX_PARCELAS = 12;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; // Permite exibir acentos e o símbolo R$

            // ===== Etapa 1: coletando dados do usuário =====

            Console.WriteLine("************* CUPOM FISCAL **************");

            // Lista que guarda todos os produtos da compra
            var itens = new List<(string Nome, decimal Preco, decimal DescontoPct, decimal ValorDesconto, decimal PrecoFinal)>();

            // Laço que permite cadastrar vários produtos
            bool adicionarOutro = true;
            while (adicionarOutro)
            {
                Console.WriteLine($"\n--- Produto {itens.Count + 1} ---");

                string nomeProduto = LerTexto("Digite o nome do produto: ");
                decimal preco = LerDecimal("Digite o preço do produto: R$ ", 0.01m, 100_000_000m);
                decimal desconto = LerDecimal("Digite o desconto em porcentagem (0 a 100): ", 0m, 100m);

                // Calcula o desconto do produto
                decimal valorDescontoItem = preco * (desconto / 100m);
                decimal precoFinalItem = preco - valorDescontoItem;

                itens.Add((nomeProduto, preco, desconto, valorDescontoItem, precoFinalItem));

                adicionarOutro = LerSimNao("Deseja adicionar outro produto? (s/n): ");
            }

            // Cliente VIP?
            Console.WriteLine();
            bool clienteVip = LerSimNao("O cliente é VIP? (s/n): ");

            // Cupom de desconto (opcional)
            bool cupomAplicado = false;
            while (true)
            {
                Console.Write("Digite o código do cupom (ou Enter para pular): ");
                string codigo = (Console.ReadLine() ?? "").Trim();

                if (codigo == "") break;

                if (codigo.Equals(CODIGO_PROMO, StringComparison.OrdinalIgnoreCase))
                {
                    cupomAplicado = true;
                    Console.WriteLine($"Cupom aplicado: {DESCONTO_PROMO}% de desconto!");
                    break;
                }
                Console.WriteLine("Cupom inválido. Tente novamente.");
            }

            // Taxa de imposto
            decimal taxaImposto = LerDecimal("Digite a taxa de imposto em porcentagem (0 a 100): ", 0m, 100m);

            // ===== Etapa 2: calculando os valores =====

            decimal totalOriginal = itens.Sum(i => i.Preco);
            decimal totalDescontosProdutos = itens.Sum(i => i.ValorDesconto);
            decimal subtotal = itens.Sum(i => i.PrecoFinal);

            // Desconto VIP aplicado sobre o subtotal
            decimal descontoVip = clienteVip ? subtotal * (DESCONTO_VIP / 100m) : 0m;
            decimal aposVip = subtotal - descontoVip;

            // Desconto do cupom aplicado sobre o valor após VIP
            decimal descontoCupom = cupomAplicado ? aposVip * (DESCONTO_PROMO / 100m) : 0m;
            decimal baseImposto = aposVip - descontoCupom;

            // Imposto somado ao valor final
            decimal valorImposto = baseImposto * (taxaImposto / 100m);
            decimal totalFinal = baseImposto + valorImposto;

            // Forma de pagamento
            Console.WriteLine($"\nTotal a pagar: {totalFinal.ToString("C", ptBR)}");
            int parcelas = 1;
            if (LerSimNao("Deseja pagar parcelado? (s/n): "))
            {
                parcelas = LerInteiro($"Quantidade de parcelas (2 a {MAX_PARCELAS}): ", 2, MAX_PARCELAS);
            }
            decimal valorParcela = Math.Round(totalFinal / parcelas, 2);

            // ===== Etapa 3: exibindo os resultados com um Cupom Fiscal =====

            Console.WriteLine("\n=====================================");
            Console.WriteLine("            CUPOM FISCAL");
            Console.WriteLine("=====================================");
            Console.WriteLine("Data/Hora: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            Console.WriteLine("-------------------------------------");

            for (int i = 0; i < itens.Count; i++)
            {
                var item = itens[i];
                Console.WriteLine($"{i + 1:00}. {item.Nome}");
                Console.WriteLine($"    Preço original: {item.Preco.ToString("C", ptBR)}");
                Console.WriteLine($"    Desconto: {item.DescontoPct}% ({item.ValorDesconto.ToString("C", ptBR)})");
                Console.WriteLine($"    Preço com desconto: {item.PrecoFinal.ToString("C", ptBR)}");
            }

            Console.WriteLine("-------------------------------------");
            Console.WriteLine($"Total original:        {totalOriginal.ToString("C", ptBR)}");
            Console.WriteLine($"Descontos produtos:   -{totalDescontosProdutos.ToString("C", ptBR)}");
            Console.WriteLine($"Subtotal:              {subtotal.ToString("C", ptBR)}");

            if (clienteVip)
                Console.WriteLine($"Desconto VIP ({DESCONTO_VIP}%):   -{descontoVip.ToString("C", ptBR)}");

            if (cupomAplicado)
                Console.WriteLine($"Cupom {CODIGO_PROMO} ({DESCONTO_PROMO}%): -{descontoCupom.ToString("C", ptBR)}");

            Console.WriteLine($"Imposto ({taxaImposto}%):     +{valorImposto.ToString("C", ptBR)}");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine($"TOTAL FINAL:           {totalFinal.ToString("C", ptBR)}");

            // Forma de pagamento
            if (parcelas == 1)
                Console.WriteLine("Pagamento: À vista");
            else
                Console.WriteLine($"Pagamento: {parcelas}x de {valorParcela.ToString("C", ptBR)}");

            // ===== Etapa 4: exibindo a mensagem final =====

            Console.WriteLine("\n-------------------------------------");
            Console.WriteLine("Obrigado por comprar conosco!");
            Console.WriteLine("******************** FIM DO CUPOM *******************");

            Console.ReadLine(); // Aguarda o usuário pressionar Enter antes de fechar o terminal
        }

        // ===== Métodos auxiliares de leitura com validação =====

        // Lê um texto que não pode ficar vazio
        static string LerTexto(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string entrada = (Console.ReadLine() ?? "").Trim();
                if (entrada != "") return entrada;
                Console.WriteLine("O campo não pode ficar vazio.");
            }
        }

        // Lê um decimal válido dentro de um intervalo (aceita vírgula ou ponto)
        static decimal LerDecimal(string mensagem, decimal minimo, decimal maximo)
        {
            while (true)
            {
                Console.Write(mensagem);
                string entrada = (Console.ReadLine() ?? "").Trim().Replace(',', '.');

                if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valor)
                    && valor >= minimo && valor <= maximo)
                {
                    return valor;
                }
                Console.WriteLine($"Valor inválido. Digite um número entre {minimo} e {maximo}.");
            }
        }

        // Lê um inteiro válido dentro de um intervalo
        static int LerInteiro(string mensagem, int minimo, int maximo)
        {
            while (true)
            {
                Console.Write(mensagem);
                if (int.TryParse(Console.ReadLine(), out int valor) && valor >= minimo && valor <= maximo)
                {
                    return valor;
                }
                Console.WriteLine($"Valor inválido. Digite um número inteiro entre {minimo} e {maximo}.");
            }
        }

        // Lê uma resposta sim/não
        static bool LerSimNao(string mensagem)
        {
            while (true)
            {
                Console.Write(mensagem);
                string resposta = (Console.ReadLine() ?? "").Trim().ToLower();
                if (resposta == "s" || resposta == "sim") return true;
                if (resposta == "n" || resposta == "nao" || resposta == "não") return false;
                Console.WriteLine("Responda com 's' ou 'n'.");
            }
        }
    }
}
