# 📋 OptimalyChat - Implementační plán

## 🎯 Přehled projektu
AI chatbot s lokálním LM Studio, učením z historie a volitelným šifrováním pro citlivé projekty.

## 🏗️ Fáze implementace

### Fáze 1: Základní infrastruktura a entity
- [ ] **1.1 Odstranit Template entity**
  - [ ] Odstranit TemplateProduct a TemplateCategory entity
  - [ ] Odstranit související DTOs, Services, ViewModels
  - [ ] Odstranit TemplateProductsController
  - [ ] Odstranit Views/TemplateProducts
  - [ ] Vytvořit migraci RemoveTemplateEntities

- [ ] **1.2 Vytvořit základní entity pro AI Chat**
  ```
  DataLayer/Entities/
  ├── Project.cs              # Projekty s volitelným šifrováním
  ├── Conversation.cs         # Konverzační vlákna
  ├── Message.cs             # Zprávy s embeddingy
  ├── Document.cs            # Nahrané dokumenty
  └── AIModel.cs             # Konfigurace AI modelů
  ```

- [ ] **1.3 EF Core konfigurace**
  - [ ] ProjectConfiguration.cs s šifrovacími políčky
  - [ ] ConversationConfiguration.cs s relacemi
  - [ ] MessageConfiguration.cs s indexy pro vyhledávání
  - [ ] Přidat DbSets do ApplicationDbContext
  - [ ] Vytvořit migraci AddAIChatEntities

### Fáze 2: Service Layer - AI a bezpečnost
- [ ] **2.1 LM Studio integrace**
  ```
  ServiceLayer/
  ├── Interfaces/
  │   ├── ILMStudioClient.cs
  │   ├── IAIService.cs
  │   └── IProjectEncryptionService.cs
  ├── Services/
  │   ├── LMStudioClient.cs      # HTTP client pro LM Studio API
  │   ├── AIService.cs           # Hlavní AI logika s kontextem
  │   └── ProjectEncryptionService.cs # Šifrování projektů
  ```

- [ ] **2.2 DTOs a mapování**
  - [ ] ProjectDto.cs s encryption status
  - [ ] ConversationDto.cs
  - [ ] MessageDto.cs
  - [ ] AIResponseDto.cs
  - [ ] Přidat mapování do MappingProfile.cs

- [ ] **2.3 Projekt management služby**
  - [ ] IProjectService.cs interface
  - [ ] ProjectService.cs implementace
  - [ ] Validace a business logika
  - [ ] Správa šifrovacích klíčů

### Fáze 3: Vektorová databáze a embeddingy
- [ ] **3.1 Vektorová databáze setup**
  - [ ] Přidat sqlite-vss NuGet balíček
  - [ ] Vytvořit VectorDbContext
  - [ ] Implementovat IVectorDatabase interface
  - [ ] SqliteVectorDatabase implementace

- [ ] **3.2 Embedding služba**
  ```
  ServiceLayer/Services/
  ├── IEmbeddingService.cs
  ├── EmbeddingService.cs     # Generování embeddingů
  ├── IVectorSearchService.cs
  └── VectorSearchService.cs  # Vyhledávání podobnosti
  ```

- [ ] **3.3 RAG implementace**
  - [ ] Indexování zpráv při uložení
  - [ ] Vyhledávání relevantního kontextu
  - [ ] Sestavení promptů s historií

### Fáze 4: Presentation Layer - UI a real-time
- [ ] **4.1 SignalR pro streaming**
  - [ ] Přidat SignalR do Program.cs
  - [ ] ChatHub.cs implementace
  - [ ] JavaScript client pro real-time
  - [ ] AI thinking indikátor

- [ ] **4.2 ViewModels**
  ```
  PresentationLayer/ViewModels/
  ├── ProjectViewModel.cs     # S encryption options
  ├── ConversationViewModel.cs
  ├── MessageViewModel.cs
  └── ChatViewModel.cs       # Pro chat interface
  ```

- [ ] **4.3 Controllers**
  - [ ] ProjectsController (CRUD + šifrování)
  - [ ] ChatController (konverzace)
  - [ ] SettingsController (AI modely)

- [ ] **4.4 Views s AdminLTE**
  ```
  Views/
  ├── Projects/
  │   ├── Index.cshtml      # Seznam projektů s 🔒/🔓
  │   ├── Create.cshtml     # Volba šifrování
  │   └── Details.cshtml    # Projekt dashboard
  ├── Chat/
  │   ├── Index.cshtml      # Chat interface
  │   └── _MessagePartial.cshtml
  └── Settings/
      └── AIModels.cshtml   # LM Studio konfigurace
  ```

### Fáze 5: Pokročilé funkce
- [ ] **5.1 Cross-thread kontext**
  - [ ] Implementovat projektové vyhledávání
  - [ ] Top-K relevantních zpráv
  - [ ] Kontextové okno management

- [ ] **5.2 Document processing**
  - [ ] Upload dokumentů do projektu
  - [ ] Text extraction služba
  - [ ] Indexování dokumentů

- [ ] **5.3 Export a import**
  - [ ] Export projektů (šifrované/nešifrované)
  - [ ] Import s dešifrováním
  - [ ] Backup/restore funkcionalita

### Fáze 6: Optimalizace a monitoring
- [ ] **6.1 Performance**
  - [ ] Caching embeddingů
  - [ ] Lazy loading konverzací
  - [ ] Background indexování

- [ ] **6.2 Monitoring**
  - [ ] Token usage tracking
  - [ ] Response time metrics
  - [ ] Error logging

- [ ] **6.3 Testing**
  - [ ] Unit testy pro services
  - [ ] Integration testy pro AI
  - [ ] E2E testy pro chat flow

## 🛠️ Technické detaily

### Konfigurace LM Studio
```json
{
  "LMStudio": {
    "BaseUrl": "http://localhost:1234/v1",
    "DefaultModel": "local-model",
    "MaxTokens": 4096,
    "Temperature": 0.7
  }
}
```

### Šifrování projektů
```csharp
public enum EncryptionLevel
{
    None = 0,           // Žádné šifrování
    MessagesOnly = 1,   // Jen zprávy
    Full = 2           // Zprávy + metadata + embeddingy
}
```

### SignalR endpoints
```javascript
// Chat hub methods
connection.invoke("SendMessage", message, projectId);
connection.invoke("CancelGeneration");
connection.on("ReceiveChunk", (chunk) => { });
connection.on("AIThinking", (thinking) => { });
```

## 📊 Databázové schéma

```sql
-- Projects
CREATE TABLE Projects (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    IsEncrypted BOOLEAN DEFAULT 0,
    EncryptionLevel INTEGER DEFAULT 0,
    CreatedAt TIMESTAMP,
    -- BaseEntity fields...
);

-- Conversations
CREATE TABLE Conversations (
    Id INTEGER PRIMARY KEY,
    ProjectId INTEGER REFERENCES Projects(Id),
    Title TEXT,
    -- BaseEntity fields...
);

-- Messages
CREATE TABLE Messages (
    Id INTEGER PRIMARY KEY,
    ConversationId INTEGER REFERENCES Conversations(Id),
    Role TEXT NOT NULL, -- 'user' or 'assistant'
    Content TEXT,
    EncryptedContent TEXT,
    Embedding BLOB, -- Vector data
    -- BaseEntity fields...
);

-- Vector index
CREATE VIRTUAL TABLE message_vectors USING vec0(
    message_id INTEGER PRIMARY KEY,
    embedding FLOAT[1536]
);
```

## 🚀 Spuštění vývoje

1. **Setup databáze**
   ```bash
   cd OptimalyChat.PresentationLayer
   dotnet ef database update
   ```

2. **Spustit LM Studio**
   - Načíst preferovaný model
   - Spustit server na portu 1234

3. **Spustit aplikaci**
   ```bash
   dotnet run
   ```

4. **Test chat interface**
   - Vytvořit nový projekt
   - Vybrat úroveň šifrování
   - Začít konverzaci

## 📝 Poznámky
- Držet se Clean Architecture principů
- Používat BaseEntity pro audit trail
- Všechny async metody s CancellationToken
- Proper error handling s custom exceptions
- AdminLTE styling pro konzistentní UI