# VisitorManagementSystem Rapor Taslağı

## Proje adı

VisitorManagementSystem

## Grup üyeleri

- Ad Soyad 1
- Ad Soyad 2
- Ad Soyad 3

## Projenin amacı

Bu projenin amacı, bir bina, kampüs, resepsiyon veya güvenlik noktası için ziyaretçi giriş-çıkışlarının dijital olarak takip edilmesini sağlayan profesyonel bir Windows Forms uygulaması geliştirmektir. Sistem ziyaretçi kaydı, ziyaretçi fotoğrafı, giriş zamanı, çıkış zamanı, ziyaret süresi, ziyaret edilen kişi ve kara liste kontrollerini merkezi bir veritabanı üzerinden yönetir.

## Kullanılan Windows Forms kontrolleri

- Label
- TextBox
- Button
- DataGridView
- ComboBox
- PictureBox
- Panel
- GroupBox tasarım yaklaşımına uygun bölümlendirilmiş paneller
- DateTimePicker
- MessageBox

## Kullanılan sınıflar

- Employee
- Visitor
- Resident
- Visit
- VisitStatus
- BlacklistEntry
- AppDbContext
- AuthService
- VisitorService
- VisitService
- ResidentService
- BlacklistService
- DurationService
- IdentityValidationService
- ImageService
- PasswordHashService
- DataMaskingService
- AppState
- ModernTheme

## Kullanılan collection yapıları

- `List<Visitor>`: ziyaretçi listelerini bellekte tutmak için kullanılmıştır.
- `List<Visit>`: aktif ve tamamlanan ziyaret listelerini ekrana hazırlamak için kullanılmıştır.
- `Dictionary<string, Visitor>`: kimlik numarasına göre hızlı ziyaretçi araması yapmak için kullanılmıştır.
- `Queue<Visit>`: giriş yapan ziyaretçileri kuyruk mantığıyla göstermek için örnek yapı olarak kullanılmıştır.
- `Stack<string>`: yapılan işlemleri son işlem en üstte olacak şekilde saklayan operasyon geçmişi için kullanılmıştır.

## Kullanılan eventler

- Button Click
- Form Load
- TextBox TextChanged
- DataGridView CellClick
- DataGridView CellDoubleClick
- MouseEnter
- MouseLeave

MouseEnter ve MouseLeave eventleri modern butonlarda görsel renk değişimi oluşturmak için kullanılmıştır.

## Veri saklama yöntemi

Projenin ilk aşamasında veriler Azure SQL Database üzerinde saklanacaktır. Böylece sistem farklı bilgisayarlardan aynı veritabanına bağlanabilecek ve ziyaretçi giriş-çıkış bilgileri ortak olarak takip edilebilecektir. Bağlantı bilgileri güvenlik nedeniyle appsettings.json dosyasında tutulacak ve bu dosya GitHub’a yüklenmeyecektir.

Entity Framework Core Code First yaklaşımı kullanılmıştır. Veritabanı tabloları model sınıflarından üretilir ve migration komutlarıyla Azure SQL Database üzerine uygulanır.

## Ekran görüntüleri

Final tesliminden önce aşağıdaki ekran görüntüleri eklenecektir:

- Login ekranı
- Dashboard ekranı
- Ziyaretçi kayıt ekranı
- Ziyaretçi giriş ekranı
- Aktif ziyaretçiler ekranı
- Ziyaret geçmişi ekranı
- Resident yönetim ekranı
- Blacklist ekranı

## Final teslimine kadar yapılacak kalan işler

- Gerçek Azure SQL şifresiyle `appsettings.json` oluşturulacak.
- Azure SQL firewall ayarları test edilecek.
- Migration komutları çalıştırılacak.
- Örnek ziyaretçi ve ziyaret kayıtları girilecek.
- Webcam ile Take Photo özelliği için uygun kütüphane seçilip TODO tamamlanacak.
- Uygulama ekran görüntüleri rapora eklenecek.
- Son testler farklı bilgisayarlardan aynı veritabanına bağlanarak yapılacak.
