namespace Domain.Enums
{
    public enum ReportStatus
    {
        New = 1,  // Yeni Kayıt
        UnderReview = 2,  // İnceleniyor
        Assigned = 3,  // Atandı
        InProgress = 4,  // Çalışılıyor
        Completed = 5,  // Tamamlandı 
        Cancelled = 6,  // İptal 
        Unfounded = 7   // Asılsız
    }
}
