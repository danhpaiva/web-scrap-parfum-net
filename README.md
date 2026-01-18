# 🕵️‍♂️ Web Scrap Parfum – Monitor de Preços .NET 10

![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Selenium](https://img.shields.io/badge/Selenium-43B02A?style=for-the-badge&logo=selenium&logoColor=white)
![Chrome](https://img.shields.io/badge/Google_Chrome-4285F4?style=for-the-badge&logo=googlechrome&logoColor=white)

Aplicação Console desenvolvida em **.NET 10** para monitoramento automatizado de preços de perfumes. O projeto utiliza **Selenium WebDriver** para realizar o scraping de múltiplas lojas, comparando valores atuais com preços base e identificando promoções em tempo real.

---

## 🚀 Tecnologias Utilizadas

- **.NET 10 (Console Application)**
- **C#**
- **Selenium WebDriver**
- **Chrome Headless Mode** (Execução em segundo plano)
- **System.Text.Json** (Manipulação de configurações)
- **Design Pattern: Strategy** (Estratégias de busca por site)
- **Design Pattern: Factory** (Criação dinâmica de Scrapers)

---

## 📦 Repositório

🔗 https://github.com/danhpaiva/web-scrap-parfum-net

---

## 🛠️ Arquitetura e Patterns

O projeto foi estruturado para ser resiliente e facilmente expansível:

- **IScraper:** Interface que padroniza o comportamento de todos os scrapers.
- **Scraper Factory:** Método centralizado para instanciar o motor de busca correto baseado no domínio da URL.
- **Resiliência:** Tratamento de erros para produtos esgotados, detecção de AJAX e camuflagem de automação para evitar bloqueios.

---

## 🛒 Lojas Suportadas

Atualmente configurado para:

1.  **Granado / Phebo**
2.  **Nuancielo**
3.  **In The Box**

---

## ⚙️ Configuração (perfumes.json)

Gerencie os itens monitorados através do arquivo JSON na raiz do projeto:

```json
[
  {
    "Nome": "Bossa - Eau de Toilette 100ml",
    "Url": "[https://www.granado.com.br/perfume-bossa-100ml/p](https://www.granado.com.br/perfume-bossa-100ml/p)",
    "PrecoBase": 230.00
  },
  {
    "Nome": "Stant Acquae - In The Box",
    "Url": "[https://www.intheboxperfumes.com.br/produto/stant-aquae-100ml-130](https://www.intheboxperfumes.com.br/produto/stant-aquae-100ml-130)",
    "PrecoBase": 169.90
  }
]


## 📄 Licença

Este projeto está licenciado sob a licença MIT.

🔗 https://github.com/danhpaiva/web-scrap-parfum-net/blob/main/LICENSE

---

## 👨‍💻 Autor

**Daniel Paiva**  
Desenvolvedor .NET

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/danhpaiva/)