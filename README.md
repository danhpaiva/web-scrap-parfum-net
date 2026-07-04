# 🕵️‍♂️ Web Scrap Parfum – Monitor de Preços .NET 10

![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Selenium](https://img.shields.io/badge/Selenium-43B02A?style=for-the-badge&logo=selenium&logoColor=white)
![Chrome](https://img.shields.io/badge/Google_Chrome-4285F4?style=for-the-badge&logo=googlechrome&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

Aplicação Console desenvolvida em **.NET 10** para monitoramento automatizado de preços de perfumes. O projeto utiliza **Selenium WebDriver** para realizar o scraping de múltiplas lojas, comparando valores atuais com o maior preço já observado e identificando promoções em tempo real. O histórico de preços é persistido em **SQLite via EF Core**, permitindo que a base de referência evolua a cada execução. Os resultados são exibidos no console e exportados automaticamente para um arquivo `.txt` na área de trabalho.

---

## 🚀 Tecnologias Utilizadas

- **.NET 10 (Console Application)**
- **C#**
- **Selenium WebDriver**
- **Chrome / Edge / Firefox Headless Mode** (fallback automático entre navegadores)
- **Entity Framework Core + SQLite** (histórico de preços e base dinâmica)
- **System.Text.Json** (leitura do seed de configurações)
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
│   ├── Persistence/       # AppDbContext, PerfumeEntity, PrecoRegistro, DatabaseInitializer
│   ├── Repositories/      # SqlitePerfumeRepository (base/histórico), JsonPerfumeRepository (seed)
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
| **Repository** | `IPerfumeRepository` abstrai a persistência; `SqlitePerfumeRepository` guarda o histórico e calcula a base |

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

O arquivo `perfumes.json` é o **seed** da lista monitorada: na primeira execução, cada produto ainda não presente no banco é importado para o SQLite, e seu `PrecoBase` vira a primeira leitura do histórico. A partir daí a base passa a ser gerida pelo banco (veja [Base Dinâmica](#-base-dinâmica-e-histórico-de-preços)). Adicione quantos produtos quiser — cada entrada requer nome, URL do produto e preço base inicial:

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

> O campo `PrecoBase` é apenas o valor **inicial**. Após a importação, a base passa a ser o maior preço já observado (persistido no banco) — o `perfumes.json` só é reconsultado para descobrir produtos novos.

---

## 💾 Base Dinâmica e Histórico de Preços

O preço de referência não é mais estático: cada execução grava uma leitura em SQLite (via EF Core, sem migrations — schema criado com `EnsureCreated`), e a **base é o maior preço já observado** para cada produto.

- **Base = maior preço observado** — sobe quando um preço mais alto é lido e nunca é sobrescrita por uma queda, então a promoção não desaparece após uma única execução.
- **Promoção** — leitura atual abaixo da base.
- **Leituras de "esgotado"** (preço `0`) são ignoradas no cálculo da base.

O banco fica em `perfumes.db`, ao lado do executável (ex.: `bin/Debug/net10.0/perfumes.db`). Para reiniciar o histórico, basta apagá-lo — ele é recriado a partir do `perfumes.json` na próxima execução.

> **Nota técnica:** o SQLite não possui tipo `decimal` (o EF o armazenaria como TEXT, tornando `MAX`/comparações lexicográficas — `"89.8" > "169.9"`). Por isso o preço é persistido em centavos (`INTEGER`) via *value converter*, garantindo comparação numérica correta.

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
4. Adicione os produtos desejados no `perfumes.json` (serão importados para o banco na próxima execução)

Nenhuma outra camada precisa ser alterada.

---

## ✅ Testes

O projeto `WebScrapParfum.Tests` (xUnit) cobre a camada de aplicação, domínio e persistência sem depender de Selenium/navegador real:

- **`ParsePrice`** (via reflection) — extração de preço em diversos formatos, incluindo separador de milhar
- **`ScrapedResult`** — cálculo de `TemDesconto` / `ValorDesconto`
- **`ScraperFactory`** — resolução de scraper por domínio e tratamento de URLs inválidas/não suportadas
- **`JsonPerfumeRepository`** — leitura do seed a partir do `perfumes.json`
- **`MonitoringService`** — orquestração do monitoramento com fakes, validando ordem dos resultados e resiliência a falhas de scraper
- **`SqlitePerfumeRepository`** — seed sem duplicação, base como maior preço observado, detecção de desconto, leituras de esgotado ignoradas e regressão da comparação numérica (não-lexicográfica) da base

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
