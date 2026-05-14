using Xunit;

/// <summary>
/// xUnit: entegrasyon testleri paylaşımlı factory kullandığından test metotları sırayla çalıştırılır
/// (filtreyle seçilen testlerin yanlışlıkla paralel koşmasını önler).
/// </summary>
[assembly: CollectionBehavior(DisableTestParallelization = true)]
