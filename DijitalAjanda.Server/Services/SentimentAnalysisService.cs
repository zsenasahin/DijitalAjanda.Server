using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;

namespace DijitalAjanda.Server.Services
{
    /// <summary>
    /// Türkçe metin duygu durum analizi servisi.
    /// Kelime bazlı hibrit yaklaşım kullanır.
    /// </summary>
    public class SentimentAnalysisService
    {
        private readonly ApplicationDbContext _context;

        // Türkçe pozitif kelimeler
        private static readonly HashSet<string> PositiveWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Duygular
            "mutlu", "mutluluk", "sevinç", "neşe", "neşeli", "keyif", "keyifli", "harika", "muhteşem", "mükemmel",
            "güzel", "süper", "fevkalade", "olağanüstü", "şahane", "enfes", "efsane", "inanılmaz",
            "huzur", "huzurlu", "sakin", "rahat", "özgür", "umut", "umutlu", "iyimser",
            
            // Başarı
            "başarı", "başardım", "başardık", "kazandım", "kazandık", "başarılı", "gurur", "gururlu",
            "tamamladım", "bitirdim", "yaptım", "ilerledim", "gelişim", "gelişme", "ilerleme",
            
            // Sosyal
            "sevgi", "seviyorum", "aşk", "arkadaş", "arkadaşlık", "dostluk", "dost", "aile",
            "minnettar", "teşekkür", "şükür", "beraber", "birlikte",
            
            // Aktiviteler
            "eğlence", "eğlendim", "keyif", "dinlendim", "tatil", "gezi", "seyahat", "kutlama",
            "hediye", "sürpriz", "parti", "festival",
            
            // Sağlık
            "sağlıklı", "enerjik", "fit", "aktif", "dinç", "zinde",
            
            // Genel pozitif
            "iyi", "iyiyim", "hoş", "tatlı", "şirin", "değerli", "özel", "benzersiz",
            "kolay", "rahatça", "sorunsuz", "problemsiz", "verimli", "üretken",
            
            // Emojiler (metin olarak)
            "😊", "😄", "😃", "🎉", "❤️", "💪", "🌟", "✨", "👍", "🥳", "😍", "🙏"
        };

        // Türkçe negatif kelimeler
        private static readonly HashSet<string> NegativeWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Duygular
            "üzgün", "üzüntü", "mutsuz", "mutsuzluk", "kötü", "berbat", "korkunç", "rezalet",
            "sinir", "sinirli", "öfke", "öfkeli", "kızgın", "stres", "stresli", "gergin",
            "kaygı", "kaygılı", "endişe", "endişeli", "korku", "korkulu", "panik",
            "depresyon", "depresif", "bunalım", "sıkıntı", "sıkıntılı", "bezgin",
            
            // Başarısızlık
            "başarısız", "başarısızlık", "kaybettim", "kaybettik", "yapamadım", "yapamıyorum",
            "beceremedim", "beceremiyorum", "sınavı", "kaldım", "reddedildim", "eledim",
            
            // Sosyal
            "yalnız", "yalnızlık", "terk", "ayrılık", "kavga", "tartışma", "küs",
            "ihanet", "hayal kırıklığı", "hayal", "kırıklığı",
            
            // Sağlık
            "hasta", "hastalık", "ağrı", "acı", "yorgun", "yorgunluk", "bitkin", "tükenmişlik",
            "uykusuz", "uykusuzluk", "baş", "ağrısı",
            
            // Genel negatif
            "zor", "zorlu", "imkansız", "umutsuz", "çaresiz", "sorun", "problem", "sıkıntı",
            "hata", "yanlış", "eksik", "yetersiz", "başarısız", "kötü", "berbat",
            "nefret", "bıktım", "usandım", "istemiyorum", "dayanamıyorum",
            
            // Emojiler (metin olarak)
            "😢", "😭", "😞", "😔", "😡", "😠", "💔", "😰", "😨", "🤢", "😤", "😫"
        };

        // Güçlendirici kelimeler (intensifiers)
        private static readonly HashSet<string> Intensifiers = new(StringComparer.OrdinalIgnoreCase)
        {
            "çok", "aşırı", "son", "derece", "gerçekten", "kesinlikle", "tamamen", "oldukça",
            "fazla", "fazlasıyla", "hiç", "asla", "her", "zaman", "sürekli", "hep"
        };

        public SentimentAnalysisService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Metni analiz eder ve sonuçları veritabanına kaydeder.
        /// </summary>
        public async Task<JournalSentiment> AnalyzeAndSaveAsync(int journalEntryId, string content)
        {
            var (label, score) = AnalyzeSentiment(content);

            var sentiment = new JournalSentiment
            {
                JournalEntryId = journalEntryId,
                SentimentLabel = label,
                SentimentScore = score,
                AnalyzedAt = DateTime.UtcNow
            };

            _context.JournalSentiments.Add(sentiment);
            await _context.SaveChangesAsync();

            return sentiment;
        }

        /// <summary>
        /// Metni analiz eder ve duygu durumunu belirler.
        /// </summary>
        public (string Label, float Score) AnalyzeSentiment(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return ("Neutral", 0.5f);
            }

            // Metni küçük harfe çevir ve kelimelere ayır
            var words = content.ToLowerInvariant()
                .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']', '"', '\'' },
                    StringSplitOptions.RemoveEmptyEntries);

            int positiveCount = 0;
            int negativeCount = 0;
            float intensifierMultiplier = 1.0f;
            bool hasIntensifier = false;

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];

                // Güçlendirici kontrolü
                if (Intensifiers.Contains(word))
                {
                    hasIntensifier = true;
                    intensifierMultiplier = 1.5f;
                    continue;
                }

                // Pozitif kelime kontrolü
                if (PositiveWords.Contains(word))
                {
                    positiveCount += hasIntensifier ? 2 : 1;
                    hasIntensifier = false;
                    intensifierMultiplier = 1.0f;
                }
                // Negatif kelime kontrolü
                else if (NegativeWords.Contains(word))
                {
                    negativeCount += hasIntensifier ? 2 : 1;
                    hasIntensifier = false;
                    intensifierMultiplier = 1.0f;
                }
                else
                {
                    // Emoji kontrolü (tekli karakterler)
                    if (PositiveWords.Any(p => word.Contains(p)))
                    {
                        positiveCount++;
                    }
                    else if (NegativeWords.Any(n => word.Contains(n)))
                    {
                        negativeCount++;
                    }
                    hasIntensifier = false;
                }
            }

            // Skor hesaplama
            int totalSentimentWords = positiveCount + negativeCount;
            
            if (totalSentimentWords == 0)
            {
                return ("Neutral", 0.5f);
            }

            // Pozitif oran: 0 ile 1 arasında
            float positiveRatio = (float)positiveCount / totalSentimentWords;
            
            // Score'u 0-1 aralığına normalize et
            // 0.5 = Nötr, > 0.5 = Pozitif, < 0.5 = Negatif
            float score = positiveRatio;

            // Kelimelerin metin uzunluğuna oranını da dikkate al
            float sentimentDensity = (float)totalSentimentWords / words.Length;
            
            // Düşük yoğunlukta nötr'e çek
            if (sentimentDensity < 0.1f)
            {
                score = 0.5f + (score - 0.5f) * 0.5f;
            }

            // Skoru 0-1 arasında sınırla
            score = Math.Clamp(score, 0.0f, 1.0f);

            // Etiketi belirle
            string label;
            if (score >= 0.6f)
            {
                label = "Positive";
            }
            else if (score <= 0.4f)
            {
                label = "Negative";
            }
            else
            {
                label = "Neutral";
            }

            return (label, score);
        }

        /// <summary>
        /// Mevcut bir günlük kaydı için sentiment günceller veya oluşturur.
        /// </summary>
        public async Task<JournalSentiment> UpdateSentimentAsync(int journalEntryId, string content)
        {
            var existingSentiment = _context.JournalSentiments
                .FirstOrDefault(js => js.JournalEntryId == journalEntryId);

            var (label, score) = AnalyzeSentiment(content);

            if (existingSentiment != null)
            {
                existingSentiment.SentimentLabel = label;
                existingSentiment.SentimentScore = score;
                existingSentiment.AnalyzedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existingSentiment;
            }
            else
            {
                return await AnalyzeAndSaveAsync(journalEntryId, content);
            }
        }
    }
}
