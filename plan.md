# OptimalyChat - Plán implementace AI Chatbota s učením z historie

## Přehled projektu
Cílem je vytvořit inteligentní chatbot, který:
- Běží lokálně pomocí LM Studio
- Učí se z historie konverzací
- Udržuje kontext napříč vlákny v rámci projektů
- Poskytuje personalizované odpovědi na základě předchozích interakcí

## Architektura s učením z historie

### 1. Struktura dat
```
Projekt
├── Konverzace/Vlákna
│   ├── Zprávy
│   └── Metadata (datum, kontext)
├── Dokumenty (nahrané soubory)
├── Poznámky
└── Embeddingy (vektorové reprezentace)
```

### 2. Technické řešení

#### Backend komponenty:
- **LM Studio API** - lokální LLM pro generování odpovědí
- **Vektorová databáze** - SQLite + sqlite-vss nebo PostgreSQL + pgvector
- **Embeddings** - menší model pro vytváření vektorových reprezentací
- **Full-text search** - pro rychlé vyhledávání v historii

#### Princip fungování:
1. **Při každé konverzaci:**
   - Uložit zprávu do databáze
   - Vytvořit embedding zprávy
   - Při dotazu najít relevantní kontext z historie

2. **Cross-thread kontext:**
   - Při nové zprávě vyhledat podobné zprávy z celého projektu
   - Přidat top-K relevantních zpráv do kontextu
   - LLM tak "vidí" relevantní historii napříč všemi vlákny

### 3. Implementační struktura

```csharp
// Základní struktura entit
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Conversation> Conversations { get; set; }
    public List<Document> Documents { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Conversation
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; }
    public List<Message> Messages { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Role { get; set; } // user/assistant
    public string Content { get; set; }
    public float[] Embedding { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Služba pro AI s kontextem
public class AIService
{
    private readonly ILMStudioClient _lmStudio;
    private readonly IVectorDatabase _vectorDb;
    private readonly IMessageRepository _messageRepo;

    public async Task<string> GetResponse(string query, int projectId)
    {
        // 1. Najít relevantní kontext z projektu
        var context = await _vectorDb.SearchSimilar(query, projectId, topK: 5);
        
        // 2. Sestavit prompt s historií
        var prompt = BuildPromptWithContext(query, context);
        
        // 3. Zavolat LM Studio
        var response = await _lmStudio.Complete(prompt);
        
        // 4. Uložit do historie s embeddingem
        await SaveToHistory(projectId, query, response);
        
        return response;
    }

    private string BuildPromptWithContext(string query, List<Message> context)
    {
        var prompt = "Na základě následující historie konverzací:\n\n";
        
        foreach (var msg in context)
        {
            prompt += $"[{msg.CreatedAt:yyyy-MM-dd}] {msg.Role}: {msg.Content}\n";
        }
        
        prompt += $"\nAktuální dotaz: {query}\n";
        prompt += "Odpověz s ohledem na předchozí kontext:";
        
        return prompt;
    }
}
```

### 4. Fáze implementace

#### Fáze 1: Základní chatbot s LM Studio
- Napojení na LM Studio API
- Základní konverzační rozhraní
- Ukládání historie do databáze

#### Fáze 2: Vektorová databáze a embeddingy
- Integrace SQLite s sqlite-vss
- Generování embeddingů pro zprávy
- Základní vyhledávání podobnosti

#### Fáze 3: Projektový kontext
- Správa projektů
- Cross-thread vyhledávání
- Vylepšené kontextové okno

#### Fáze 4: Pokročilé funkce
- Import dokumentů do kontextu
- Učení z uživatelských preferencí
- Export a sdílení projektů

### 5. Výhody tohoto přístupu
- **Personalizace**: AI se učí z tvých předchozích konverzací
- **Kontinuita**: Kontext se přenáší mezi vlákny
- **Lokální provoz**: Vše běží na tvém stroji, data zůstávají privátní
- **Škálovatelnost**: Lze později rozšířit o cloud podporu

### 6. Technologický stack
- **Backend**: ASP.NET Core 8.0
- **Databáze**: SQLite + sqlite-vss (vektorové vyhledávání)
- **AI**: LM Studio (lokální LLM)
- **Frontend**: Blazor/Razor Pages s real-time updaty
- **Embeddings**: SentenceTransformers nebo menší BERT model

### 7. Alternativní řešení pro vektorovou databázi
- **PostgreSQL + pgvector**: Pokud preferuješ robustnější řešení
- **Qdrant**: Specializovaná vektorová databáze
- **ChromaDB**: Jednoduché embeddings databáze
- **In-memory**: Pro menší projekty (FAISS wrapper)

## Další klíčové funkce

### 8. Streaming odpovědí
Real-time zobrazování odpovědí pro lepší uživatelský zážitek:

#### Implementace:
```csharp
// SignalR Hub pro real-time komunikaci
public class ChatHub : Hub
{
    private readonly IAIService _aiService;
    
    public async Task SendMessage(string message, int projectId)
    {
        var cancellationToken = Context.ConnectionAborted;
        
        // Poslat indikátor že AI přemýšlí
        await Clients.Caller.SendAsync("AIThinking", true);
        
        // Stream odpovědi po částech
        await foreach (var chunk in _aiService.StreamResponse(message, projectId, cancellationToken))
        {
            await Clients.Caller.SendAsync("ReceiveChunk", chunk);
        }
        
        await Clients.Caller.SendAsync("AIThinking", false);
        await Clients.Caller.SendAsync("MessageComplete");
    }
    
    public async Task CancelGeneration()
    {
        // Zrušit aktuální generování
        Context.Abort();
    }
}

// AI Service s podporou streamingu
public class AIService
{
    public async IAsyncEnumerable<string> StreamResponse(
        string query, 
        int projectId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = await GetRelevantContext(query, projectId);
        var prompt = BuildPromptWithContext(query, context);
        
        // LM Studio API stream
        var stream = _lmStudio.StreamCompletion(prompt, cancellationToken);
        
        var buffer = new StringBuilder();
        await foreach (var token in stream)
        {
            buffer.Append(token);
            
            // Posílat po slovech nebo větách
            if (token.Contains(" ") || token.Contains("."))
            {
                yield return buffer.ToString();
                buffer.Clear();
            }
        }
        
        if (buffer.Length > 0)
            yield return buffer.ToString();
    }
}
```

#### Frontend (JavaScript):
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

let currentMessage = "";
let isGenerating = false;

// Přijímat chunky
connection.on("ReceiveChunk", (chunk) => {
    currentMessage += chunk;
    updateMessageDisplay(currentMessage);
});

connection.on("AIThinking", (thinking) => {
    showThinkingIndicator(thinking);
});

connection.on("MessageComplete", () => {
    isGenerating = false;
    enableSendButton();
});

// Zrušit generování
function cancelGeneration() {
    if (isGenerating) {
        connection.invoke("CancelGeneration");
        isGenerating = false;
    }
}
```

### 9. Šifrování a bezpečnost dat

#### Koncept šifrování citlivých dat:
```csharp
public class EncryptionService
{
    private readonly IConfiguration _config;
    
    // Šifrování na úrovni projektu - každý projekt má svůj klíč
    public class ProjectEncryption
    {
        // Generovat unikátní klíč pro každý projekt
        public string GenerateProjectKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[32]; // 256-bit key
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }
        
        // Šifrovat zprávu pomocí AES-256-GCM
        public EncryptedMessage EncryptMessage(string content, string projectKey)
        {
            var key = Convert.FromBase64String(projectKey);
            using var aesGcm = new AesGcm(key);
            
            var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
            RandomNumberGenerator.Fill(nonce);
            
            var plaintextBytes = Encoding.UTF8.GetBytes(content);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];
            
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            
            return new EncryptedMessage
            {
                Ciphertext = Convert.ToBase64String(ciphertext),
                Nonce = Convert.ToBase64String(nonce),
                Tag = Convert.ToBase64String(tag)
            };
        }
        
        // Dešifrovat zprávu
        public string DecryptMessage(EncryptedMessage encrypted, string projectKey)
        {
            var key = Convert.FromBase64String(projectKey);
            using var aesGcm = new AesGcm(key);
            
            var ciphertext = Convert.FromBase64String(encrypted.Ciphertext);
            var nonce = Convert.FromBase64String(encrypted.Nonce);
            var tag = Convert.FromBase64String(encrypted.Tag);
            
            var plaintextBytes = new byte[ciphertext.Length];
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
            
            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
    
    // Anonymizace osobních údajů
    public class DataAnonymization
    {
        private readonly List<Regex> _patterns = new()
        {
            new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"), // Email
            new Regex(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b"), // Phone US
            new Regex(@"\b\d{9}\b"), // Rodné číslo
            new Regex(@"\b(?:\d{4}[\s-]?){3}\d{4}\b"), // Credit card
        };
        
        public string AnonymizeText(string text)
        {
            foreach (var pattern in _patterns)
            {
                text = pattern.Replace(text, "[REDACTED]");
            }
            return text;
        }
    }
}

// Použití v databázi
public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } // Toto bude šifrované
    public string EncryptedContent { get; set; }
    public string Nonce { get; set; }
    public string Tag { get; set; }
    public bool IsEncrypted { get; set; }
}
```

#### Klíčové koncepty šifrování:

1. **End-to-end šifrování**: Zprávy jsou šifrovány na klientovi a dešifrovány až při zobrazení
2. **Per-project klíče**: Každý projekt má vlastní šifrovací klíč
3. **Key management**: Klíče mohou být uloženy v:
   - Systémovém keychainingu (macOS Keychain, Windows Credential Store)
   - Hardware security module (HSM)
   - Encrypted database s master key
4. **Zero-knowledge**: Server nikdy nevidí nešifrovaná data
5. **Compliance**: GDPR-ready s možností kompletního smazání dat

### 10. Volitelné šifrování na úrovni projektu

#### Implementace:
```csharp
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEncrypted { get; set; } // Uživatel rozhodne
    public string? EncryptionKey { get; set; } // Null pokud není šifrovaný
    public EncryptionLevel EncryptionLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum EncryptionLevel
{
    None = 0,           // Žádné šifrování
    MessagesOnly = 1,   // Jen zprávy
    Full = 2           // Zprávy + metadata + embeddingy
}

// Service pro správu šifrování
public class ProjectEncryptionService
{
    public async Task<Project> CreateProject(string name, bool enableEncryption, EncryptionLevel level = EncryptionLevel.MessagesOnly)
    {
        var project = new Project
        {
            Name = name,
            IsEncrypted = enableEncryption,
            EncryptionLevel = enableEncryption ? level : EncryptionLevel.None,
            CreatedAt = DateTime.UtcNow
        };
        
        if (enableEncryption)
        {
            project.EncryptionKey = GenerateProjectKey();
            // Bezpečně uložit klíč do keychainu
            await StoreKeyInKeychain(project.Id, project.EncryptionKey);
        }
        
        return project;
    }
    
    // Wrapper pro ukládání zpráv
    public async Task<Message> SaveMessage(Message message, Project project)
    {
        if (project.IsEncrypted)
        {
            var encrypted = EncryptMessage(message.Content, project.EncryptionKey);
            message.EncryptedContent = encrypted.Ciphertext;
            message.Nonce = encrypted.Nonce;
            message.Tag = encrypted.Tag;
            message.Content = null; // Neukladat plain text
        }
        
        return await _repository.SaveMessage(message);
    }
    
    // UI Helper - zobrazit status
    public string GetProjectSecurityStatus(Project project)
    {
        return project.IsEncrypted 
            ? $"🔒 Šifrováno ({project.EncryptionLevel})" 
            : "🔓 Nešifrováno";
    }
}
```

#### UI pro vytvoření projektu:
```html
<!-- Create Project Modal -->
<div class="modal">
    <h3>Nový projekt</h3>
    
    <input type="text" @bind="projectName" placeholder="Název projektu" />
    
    <div class="encryption-section">
        <label>
            <input type="checkbox" @bind="enableEncryption" />
            <span>🔒 Povolit šifrování</span>
        </label>
        
        @if (enableEncryption)
        {
            <div class="encryption-options">
                <select @bind="encryptionLevel">
                    <option value="MessagesOnly">Pouze zprávy</option>
                    <option value="Full">Plné šifrování (včetně metadat)</option>
                </select>
                
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i>
                    Šifrovaná data jsou chráněna AES-256-GCM. 
                    Klíč bude bezpečně uložen ve vašem systému.
                </div>
            </div>
        }
    </div>
    
    <button @onclick="CreateProject">Vytvořit projekt</button>
</div>
```

#### Výhody volitelného šifrování:

1. **Flexibilita**: 
   - Osobní projekty = bez šifrování (rychlejší)
   - Firemní/citlivé = šifrované

2. **Performance**:
   - Nešifrované projekty jsou rychlejší
   - Vektorové vyhledávání funguje lépe bez šifrování

3. **Použití**:
   ```
   📁 Moje projekty
   ├── 🔓 Učení programování (nešifrováno)
   ├── 🔓 Osobní poznámky (nešifrováno)
   ├── 🔒 Firemní strategie (šifrováno)
   └── 🔒 Zdravotní záznamy (plně šifrováno)
   ```

4. **Migrace**:
   - Možnost dodatečně zapnout šifrování
   - Export nešifrovaných dat před šifrováním