# .NET 後端工程師實作考題

這是一個 .NET 工程師線上作業要求所建置的 ASP.NET Core Web API 專案。

## 專案核心功能

* **幣別資料維護 (CRUD):** 提供一組 RESTful API，用於對「幣別與其中文名稱」進行新增、查詢、修改、刪除操作。
* **CoinDesk API 整合:**
    * 呼叫外部 CoinDesk API (`https://api.coindesk.com/v1/bpi/currentprice.json`) 以獲取即時的價格。
    * 實作一個新的 API，將從 CoinDesk 取得的資料與本地資料庫中的幣別中文名稱進行整合與轉換，提供一個全新的、符合需求的資料格式。
* **單元測試:** 專案中所有核心的業務邏輯服務 (`CurrencyService`, `CoinDeskService`) 均涵蓋了完整的單元測試，以確保程式碼的品質與穩定性。

## 功能展示 (Demo)

可以透過以下連結觀看本專案所有功能的實際操作影片：

[**觀看 Demo 影片 (YouTube)**](https://youtu.be/-ziN9J-cHBk)

## 技術棧 (Technology Stack)

| 類別                 | 技術                        | 用途                                                 |
| :------------------- | :-------------------------- | :--------------------------------------------------- |
| **核心框架** | .NET 8                      | 專案的基礎開發框架。                                 |
| **Web API** | ASP.NET Core 8.0            | 用於建置高效能的 RESTful API。                       |
| **資料庫** | SQL Server Express LocalDB  | 本地開發所使用的資料庫。                             |
| **ORM** | Entity Framework Core 8.0   | 用於物件與關聯式資料庫之間的對應與操作。             |
| **日誌** | NLog                        | 提供強大且具彈性的結構化日誌記錄功能。               |
| **測試框架** | xUnit                       | 用於撰寫單元測試案例。                               |
| **模擬框架** | Moq                         | 在單元測試中建立模擬物件，以隔離待測目標。           |
| **容器化** | Docker                      | 提供容器化部署的能力，確保環境一致性。               |

## 如何開始 (Getting Started)

### 前置需求

* .NET 8 SDK
* Visual Studio 2022
* SQL Server Express LocalDB
* (可選) Docker Desktop

### 安裝與執行

1.  **開啟專案:**
    使用 Visual Studio 2022 開啟 `.sln` 方案檔。
2.  **還原 NuGet 套件:**
    專案應會自動還原套件，若無，請在「套件管理器主控台」中執行 `Update-Package -reinstall`。
3.  **建立資料庫:**
    * 在「套件管理器主控台」中，確認「預設專案」為 `CathayBank.API`。
    * 依序執行以下指令來建立資料庫與資料表：
        ```powershell
        Add-Migration [Migration名稱]
        Update-Database
        ```
4.  **執行專案:**
    按下 `F5` 或點擊 Visual Studio 中的執行按鈕，專案啟動後會自動打開 Swagger UI 頁面。

## API 端點說明

專案啟動後，可透過 `https://localhost:[port]/swagger` 訪問 API 文件頁面。

* `GET /api/coindesk/currentprice`: 取得 CoinDesk API 的原始回應。
* `GET /api/coindesk/transformed-price`: 取得整合轉換後的新 API 資料。
* `GET /api/currencies`: 取得所有幣別資料。
* `POST /api/currencies`: 新增一筆幣別資料。
* `GET /api/currencies/{code}`: 取得指定代碼的幣別資料。
* `PUT /api/currencies/{code}`: 修改指定代碼的幣別資料。
* `DELETE /api/currencies/{code}`: 刪除指定代碼的幣別資料。

## 專案架構說明

```
/ (Solution Root)
|
├── CathayBank.API/      # 主應用程式專案
|   |
|   ├── Controllers/     # API 控制器層：負責接收 HTTP 請求並回傳結果。
|   |   ├── CurrenciesController.cs
|   |   └── CoinDeskController.cs
|   |
|   ├── Data/            # 資料存取層
|   |   └── ApiDbContext.cs  # Entity Framework Core 的資料庫上下文。
|   |
|   ├── Handlers/        # HTTP 訊息處理常式
|   |   └── LoggingDelegatingHandler.cs # 用於攔截並記錄對外發出的 HTTP 請求。
|   |
|   ├── Middlewares/     # ASP.NET Core 中介軟體
|   |   └── UnifiedDiagnosticsMiddleware.cs # 統一處理全域例外與請求/回應日誌。
|   |
|   ├── Models/          # 資料模型與 DTO
|   |   ├── Currency.cs      # 資料庫實體。
|   |   └── ...Dtos.cs     # API 的資料傳輸物件。
|   |
|   ├── Resources/       # 多語系資源檔
|   |   ├── SharedResource.zh-TW.json
|   |   └── SharedResource.en-US.json
|   |
|   ├── Services/        # 業務邏輯服務層
|   |   ├── ICoinDeskService.cs  # CoinDesk 服務的介面。
|   |   ├── ICurrencyService.cs  # 幣別服務的介面。
|   |   ├── CoinDeskService.cs   # 實作 ICoinDeskService，處理與 CoinDesk API 的互動。
|   |   ├── CurrencyService.cs   # 實作 ICurrencyService，處理幣別資料的 CRUD 與加解密。
|   |   ├── CryptoService.cs     # 封裝 AES 加解密邏輯。
|   |   └── JsonStringLocalizer.cs # 手動讀取 JSON 檔案以實現多語系功能。
|   |
|   ├── appsettings.json # 主要設定檔。
|   ├── nlog.config      # NLog 日誌設定檔。
|   ├── Dockerfile       # Docker 容器化設定檔。
|   └── Program.cs       # 應用程式進入點與服務註冊。
|
└── CathayBank.Tests/    # 單元測試專案
    ├── CurrencyServiceTests.cs  # 針對 CurrencyService 的單元測試。
    └── CoinDeskServiceTests.cs  # 針對 CoinDeskService 的單元測試。
```

## 加分項目實作說明

本專案完成了以下所有加分項目，具體實作方式說明如下：

1.  **日誌記錄 (Logging)**
    * **實作方式:** 透過客製化的 `UnifiedDiagnosticsMiddleware` 與 `LoggingDelegatingHandler`，並整合 **NLog** 框架。
    * **說明:** `UnifiedDiagnosticsMiddleware` 負責記錄所有傳入 API 的請求與最終回應的 Body；`LoggingDelegatingHandler` 則攔截對外呼叫 CoinDesk API 的請求與回應。所有日誌均透過 NLog 寫入檔案與主控台，提供了完整的請求追蹤能力。

2.  **錯誤處理 (Error Handling)**
    * **實作方式:** 在 `UnifiedDiagnosticsMiddleware` 中實作了全域例外處理機制。
    * **說明:** 此中介軟體能捕獲專案中任何未處理的例外，記錄詳細錯誤後，回傳一個格式統一、對前端友善的 JSON 錯誤訊息，避免了系統內部錯誤細節的外洩。

3.  **多語系設計 (Multi-language Support)**
    * **實作方式:** 使用 `JSON` 檔案儲存翻譯字串，並在 `UnifiedDiagnosticsMiddleware` 中根據請求的 `Accept-Language` Header 動態讀取對應的錯誤訊息。
    * **說明:** 當 API 發生錯誤時，能夠根據客戶端的語言偏好（支援 `zh-TW` 與 `en-US`），回傳在地化的錯誤訊息，提升了國際化應用的使用者體驗。

4.  **設計模式實作 (Design Patterns)**
    * **依賴注入 (Dependency Injection):** 專案大量使用建構函式注入，將服務與其依賴解耦，是整個專案的核心設計原則。
    * **中介軟體 (Middleware):** `UnifiedDiagnosticsMiddleware` 是此模式的具體實踐，將橫切關注點從業務邏輯中分離。
    * **倉儲模式 (Repository):** `CurrencyService` 封裝了資料存取邏輯，扮演了倉儲的角色。
    * **選項模式 (Options):** 透過 `IOptions<CryptoSettings>` 將加解密設定從程式碼中分離，方便管理。

5.  **Docker 支援**
    * **實作方式:** 在專案根目錄下提供了 `Dockerfile`。
    * **說明:** 採用了「多階段建置 (multi-stage build)」的最佳實踐，最終產生的映像檔體積小、安全性高，可以在任何支援 Docker 的環境中快速部署與執行。

6.  **加解密技術應用 (Encryption)**
    * **實作方式:** 建立了 `CryptoService`，使用 **AES** 演算法，並將其整合進 `CurrencyService` 中。
    * **說明:** 在新增或修改幣別資料時，會自動將 `ChineseName` 欄位加密後再存入資料庫；查詢時則會自動解密後再回傳。這展示了如何在應用程式中實作「靜態資料加密 (Data at Rest)」，保護資料庫中資料的機密性。

## 資料庫結構 (Database Schema)

```sql
CREATE TABLE Currencies (
    Code NVARCHAR(10) NOT NULL PRIMARY KEY,
    ChineseName NVARCHAR(256) NOT NULL
);
