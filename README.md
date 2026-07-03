# 🕵️‍♂️ Web Scrap Parfum – Monitor de Preços .NET 10

![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Selenium](https://img.shields.io/badge/Selenium-43B02A?style=for-the-badge&logo=selenium&logoColor=white)
![Chrome](https://img.shields.io/badge/Google_Chrome-4285F4?style=for-the-badge&logo=googlechrome&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

Aplicação Console desenvolvida em **.NET 10** para monitoramento automatizado de preços de perfumes. O projeto utiliza **Selenium WebDriver** para realizar o scraping de múltiplas lojas, comparando valores atuais com preços base e identificando promoções em tempo real. Os resultados são exibidos no console e exportados automaticamente para um arquivo `.txt` na área de trabalho.

---

## 🚀 Tecnologias Utilizadas

- **.NET 10 (Console Application)**
- **C#**
- **Selenium WebDriver**
- **Chrome / Edge / Firefox Headless Mode** (fallback automático entre navegadores)
- **System.Text.Json** (leitura de configurações)
- **Processamento Paralelo** (`Parallel.ForEach` com grau máximo 3)
- **Docker** (containerização pronta para produção)

---

## 📦 Repositório

🔗 https://github.com/danhpaiva/web-scrap-parfum-net

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture**, com responsabilidades separadas em camadas independentes:

```
WebScrapParfum.Console/
├── Domain/
│   ├── Entities/          # PerfumeConfig
│   └── ValueObjects/      # ScrapedResult (desconto calculado automaticamente)
│
├── Application/
│   ├── Interfaces/        # IScraper, IScraperFactory, IPerfumeRepository, INotifier
│   └── Services/          # MonitoringService (orquestra o loop de monitoramento)
│
├── Infrastructure/
│   ├── Factories/         # ScraperFactory, WebDriverFactory, DriverSettings
│   ├── Repositories/      # JsonPerfumeRepository
│   └── Scrapers/          # ScraperBase + um scraper por loja
│
├── Presentation/
│   └── Notifiers/         # ConsoleNotifier, FileNotifier, CompositeNotifier
│
└── Program.cs             # Composição das dependências (wiring)
```

### Design Patterns aplicados

| Pattern | Onde |
|---|---|
| **Strategy** | Cada loja tem seu próprio scraper com estratégia de extração independente |
| **Factory** | `ScraperFactory` instancia o scraper correto pelo domínio da URL |
| **Composite** | `CompositeNotifier` delega para múltiplos notifiers simultaneamente |
| **Template Method** | `ScraperBase` define o ciclo de vida; subclasses implementam apenas `Monitorar()` |

---

## 🌐 Seleção de Navegador

O `WebDriverFactory` tenta os navegadores na seguinte ordem, usando o primeiro disponível na máquina:

1. **Google Chrome**
2. **Microsoft Edge** (Chromium — mesmas opções do Chrome)
3. **Mozilla Firefox** (com mapeamento de preferências equivalentes)

Nenhuma configuração manual é necessária — a detecção é automática.

---

## 🛒 Lojas Suportadas

| # | Loja | Domínio |
|---|---|---|
| 1 | Granado / Phebo | `granado.com.br` |
| 2 | Nuancielo | `nuancielo.com.br` |
| 3 | In The Box | `intheboxperfumes.com.br` |
| 4 | Natura | `natura.com.br` |
| 5 | Avatim | `avatim.com.br` |
| 6 | Amazon | `amazon.com.br` |
| 7 | Zara | `zara.com` |
| 8 | O Boticário | `boticario.com.br` |
| 9 | Thera Cosméticos | `theracosmeticos.com.br` |
| 10 | Mahogany | `mahogany.com.br` |
| 11 | Maison Viegas | `maisonviegas.com.br` |
| 12 | Mercado Livre | `mercadolivre.com.br` |
| 13 | Wepink | `wepink.com.br` |
| 14 | Eudora | `eudora.com.br` |
| 15 | Perfumistta | `perfumistta.com.br` |

---

## ⚙️ Configuração (`perfumes.json`)

Gerencie os itens monitorados através do arquivo `perfumes.json` na raiz do projeto publicado. Adicione quantos produtos quiser — cada entrada requer nome, URL do produto e preço base de referência:

```json
[
  {
    "Nome": "Bossa - Eau de Toilette 100ml",
    "Url": "https://www.granado.com.br/granado/eau-de-toilette-bossa-100ml",
    "PrecoBase": 195.00
  },
  {
    "Nome": "Infinite Horizon - In The Box 100ml",
    "Url": "https://www.intheboxperfumes.com.br/produto/infinite-horizon-100ml-241",
    "PrecoBase": 189.90
  }
]
```

> O campo `PrecoBase` define o valor de referência. Quando o preço atual for inferior a ele, o sistema identifica e destaca a promoção.

---

## 📤 Saída dos Resultados

Os resultados são entregues simultaneamente em dois destinos via `CompositeNotifier`:

- **Console** — saída colorida em tempo real (verde = promoção, magenta = esgotado, amarelo = erro)
- **Arquivo `.txt`** — salvo automaticamente na área de trabalho com o nome `lista_perfumes_YYYY-MM-DD.txt`

Para adicionar um novo destino (ex: Telegram, e-mail), basta implementar `INotifier` e incluí-lo no `CompositeNotifier` em `Program.cs`.

---

## 🐳 Docker

O projeto inclui um `Dockerfile` com build multi-stage otimizado para produção:

```bash
# Build da imagem
docker build -t web-scrap-parfum .

# Execução
docker run --rm web-scrap-parfum
```

---

## ➕ Adicionando uma Nova Loja

1. Crie `Infrastructure/Scrapers/NomeDaLojaScraper.cs` herdando de `ScraperBase`
2. Implemente o método `Monitorar(PerfumeConfig config)`
3. Registre o domínio em `Infrastructure/Factories/ScraperFactory.cs`
4. Adicione os produtos desejados no `perfumes.json`

Nenhuma outra camada precisa ser alterada.

---

## ✅ Testes

O projeto `WebScrapParfum.Tests` (xUnit) cobre a camada de aplicação e domínio sem depender de Selenium/navegador real:

- **`ParsePrice`** (via reflection) — extração de preço em diversos formatos, incluindo separador de milhar
- **`ScrapedResult`** — cálculo de `TemDesconto` / `ValorDesconto`
- **`ScraperFactory`** — resolução de scraper por domínio e tratamento de URLs inválidas/não suportadas
- **`JsonPerfumeRepository`** — leitura de configuração a partir do `perfumes.json`
- **`MonitoringService`** — orquestração do monitoramento com fakes, validando ordem dos resultados e resiliência a falhas de scraper

Para rodar os testes:

```bash
dotnet test WebScrapParfum.Console/WebScrapParfum.Tests
```

---

## 📄 Licença

Este projeto está licenciado sob a licença MIT.

🔗 https://github.com/danhpaiva/web-scrap-parfum-net/blob/main/LICENSE

---

## 👨‍💻 Autor

**Daniel Paiva**  
Desenvolvedor .NET

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/danhpaiva/)
