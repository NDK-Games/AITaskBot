class DailyReport
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime ReportDate { get; set; }   // Из строки "Дата:"
    public int HoursWorked { get; set; }       // Из строки "Часов:"
    public string? FullReportText { get; set; } // Полный текст отчёта (в памяти)
    public DateTime ReceivedAt { get; set; }   // Когда получен (дата/время сервера)
}