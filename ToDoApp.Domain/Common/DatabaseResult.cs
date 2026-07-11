namespace ToDoApp.Domain.Common;

public enum DatabaseResult
{
    Success,          // Operacija (vnos/posodobitev/izbris) je uspela
    NoChanges,        // Pri posodabljanju uporabnik ni spremenil ničesar
    AlreadyExists,    // Pri kreiranju/urejanju podatek že obstaja (kršitev unikatnosti)
    NotFound,         // Entitete ni mogoče najti (koristno za Delete operacije)
    Failed            // Splošna tehnična odpoved ali zavrnitev s strani baze
}