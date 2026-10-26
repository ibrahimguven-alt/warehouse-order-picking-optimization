# İbon'un Deposu | Warehouse Order Picking & Route Planning

**Depo içi sipariş toplama rotalarını modelleyen, görselleştiren ve farklı rota yaklaşımlarını karşılaştıran C# ve Python projesi.**

Bir ders kapsamında C# / Windows Forms ile başlayan bu çalışma, yaklaşık bir yıldır üzerinde çalıştığım ve geliştirmeye devam ettiğim bir endüstri mühendisliği projesidir. Python sürümüyle modelleme, algoritma ve görselleştirme bileşenlerini ayrı modüllere taşıyorum.

Projenin odağı, seçilen ürün lokasyonları için depo koridorları üzerinde hesaplanan bir toplama rotası üretmektir. Görselleştirme, hesaplanan rota noktalarını gösterir; asıl karar mantığı rota hesaplama bileşenlerindedir.

## Mühendislik problemi

Sipariş toplamada operatörün hangi koridora gireceği, nereden geri döneceği ve ürünleri hangi sırayla ziyaret edeceği yürüyüş mesafesini etkiler. Bu proje şu akışı somutlaştırır:

1. Blok, koridor ve lokasyon parametreleriyle depo düzenini oluştur.
2. Manuel seçim veya rastgele sipariş ile ürün lokasyonlarını belirle.
3. Bir rota yöntemi seç ve kapalı toplama güzergâhını hesapla.
4. Rota noktalarını ve mesafesini incele; farklı yöntemleri karşılaştır.

Amaç fonksiyonu rota mesafesidir. Depo geometrisi koridor erişimini tanımlar. Raf seviyesi bilgisi ile yatay rota mesafesi aynı şey değildir; bu sürüm dikey toplama süresini kapsamlı biçimde optimize etmez.

## Sürümler ve özellikler

| Alan | C# / Windows Forms | Python |
| --- | --- | --- |
| Depo modeli | Parametrik 2D depo, raf/lokasyon ve kat seçimi | Parametrik depo modeli ve Tkinter 2D arayüz |
| Sipariş | Manuel ve rastgele seçim | Manuel ve rastgele seçim; çekirdekte seed desteği |
| Rota | Manuel rota, S-shape, Combined ve Optimal etiketli hesaplamalar | S-Shape, Combined+, Roodbergen etiketli hesaplamalar |
| Karşılaştırma | Rota sonuçları ve kayıtlar | Seçilen yöntemin rota noktaları ve mesafesi |
| Yönetim | Kullanıcı/rol işlemleri, SQLite, rota istatistikleri | C# yönetim özelliklerinin tamamı henüz taşınmadı |
| Maliyet | Araç ayarları üzerinden yakıt/elektrik maliyeti tahminleri | Henüz taşınmadı |
| 3D | Bulunmuyor | Ursina ile depo gezintisi prototipi |

Maliyet ekranları ayarlara dayalı tahminlerdir; gerçek işletmeden ölçülmüş tasarruf sonuçları değildir.

## Rota algoritmaları ve uygulama kapsamı

### Ratliff–Rosenthal temel alınarak geliştirilen rota yaklaşımı

C# kodunda `RunRatliffRosenthalRoute` ve `BuildRatliffRosenthalRoute` adlı yöntemler bulunur. İki çapraz koridor bulunan düzende algoritma, ürünlerin koridorlara dağılımını ve koridor içindeki boşlukları değerlendirerek önden girip çıkma ve döngü adayları oluşturur; üretilen adaylar arasından düşük maliyetli olanı seçer.

Bu uygulama, klasik Ratliff–Rosenthal algoritmasının tüm durumlarını eksiksiz gerçekleştirdiği bağımsız olarak doğrulanmış bir referans çözücü olarak sunulmaz. Arayüzdeki **Optimal** etiketi tek başına tüm depo düzenleri ve siparişler için global optimalite kanıtı değildir. Aday kümesi içindeki en iyi rota ile tüm uygulanabilir rotalar içindeki en iyi rota birbirinden ayrılmalıdır.

### Çoklu çapraz koridor: C# kapalı tur çözümü

C# sürümünde ikiden fazla çapraz koridor için ağ mesafeleri hesaplanır ve oluşan uzaklık matrisi üzerinde kapalı tur dinamik programlama çözümü kullanılır. Kodda bu dal için 18 ürün sınırı vardır. Bu dalın Python'a tam aktarımı ve sürümler arası doğrulama yol haritasındadır.

### S-Shape ve Combined+

- **S-Shape:** Ürün bulunan koridorları dönüşümlü yönlerde ziyaret eden rota yaklaşımıdır.
- **Combined+:** Üstte/altta kalma durumlarını ve koridordan geçme/geri dönme seçeneklerini maliyetlerle karşılaştıran dinamik programlama yapısını kullanır.
- **Python Roodbergen etiketi:** `roodbergen_route` önden gir-çık ve döngü adaylarını karşılaştırır. Etiket, Roodbergen literatüründeki tüm algoritmaların eksiksiz uygulandığı anlamına gelmez.

C# arayüzündeki **Largest Gap** ve **Aisle-by-aisle** düğmeleri mevcut kodda S-Shape işleyicisini; **Combined+** ise Combined işleyicisini paylaşır. Bunlar bağımsız uygulanmış algoritmalar olarak değerlendirilmemelidir.

### Mesafe birimi

Python `distance_raw`, rota üzerindeki mutlak yatay ve düşey koordinat farklarını toplar. `distance`, bu değeri mevcut sabit ölçek olan 18'e bölüp bir ondalığa yuvarlar. Sonuç doğrudan gerçek metre ölçümü olarak yorumlanmamalıdır. Fiziksel mesafe raporu için koordinat ölçeğinin gerçek depo ölçüleriyle kalibre edilmesi gerekir.

## Mimari

```text
ibonundeposu.formapp/   C# Windows Forms uygulaması ve SQLite servisleri
python_version/
  config/              Depo parametreleri
  warehouse/           Depo modeli ve yerleşim üretimi
  orders/              Sipariş veri modeli
  routing/             Rota yöntemleri ve mesafe hesabı
  ui/                  Tkinter arayüzü ve 3D bilgi ekranı
  visualization3d/     Ursina sahnesi, kamera ve hareket kontrolü
  main.py              2D uygulama başlangıcı
  main_3d.py           3D prototip başlangıcı
tests/                 Rota geometrisi regresyon kontrolü
```

## Python ile çalıştırma

Python **3.10 veya üzeri** gerekir. 2D uygulama standart kütüphanedeki Tkinter'ı kullanır; bazı Linux kurulumlarında Tkinter ayrıca kurulmalıdır.

```bash
cd python_version
python main.py
```

3D prototip için:

```bash
python -m pip install -r requirements.txt
python main_3d.py
```

2D ekranında depo parametrelerini gir, **Depoyu oluştur** seçeneğine bas, sipariş oluştur ve rota yöntemini seç.

3D kontroller: WASD hareket, fare bakış, Shift hızlı hareket, F kamera modu, R rota çizgisi, M ürün işaretleri, T başlangıca dönüş, Esc fare kilidi.

**3D durumu:** Depo içinde gezinme için başlangıç prototipi mevcuttur. 2D ekranda seçilen güncel depo/sipariş/rota durumu 3D sürecine aktarılmıyor; 3D kendi varsayılan düzeni ve örnek siparişiyle başlıyor. Gerçek depo hissi veren ve güncel rota ile bütünleşen 3D deneyimi geliştirmeye devam ediyorum.

## C# ile çalıştırma

Windows, Visual Studio'nun .NET masaüstü geliştirme bileşenleri ve **.NET Framework 4.7.2 Developer Pack** gerekir. `ibonundeposu.formapp/ibonundeposu.formapp.csproj` dosyasını açıp derleyin. SQLite kitaplıkları `lib/` klasöründe tutulmuştur; dağıtımda ilgili üçüncü taraf lisans koşulları ayrıca korunmalıdır.

Yönetici hesabının eski sabit parolası paylaşım kopyasından kaldırılmıştır. Uygulamayı başlatmadan önce `IBON_ADMIN_PASSWORD` ortam değişkenini kendi parolanızla tanımlayın; yönetici kullanıcı adı `admin` olarak kalır. Ortam değişkeni tanımlı değilse yönetici girişi kapalıdır. Çalışan bir Visual Studio süreci yeni ortam değişkenini almak için yeniden başlatılmalıdır.

Bu depo eğitim ve geliştirme amaçlıdır. C# kullanıcı parola saklama ve kimlik doğrulama tasarımı üretim kullanımı öncesinde ayrıca gözden geçirilmelidir.

## Doğrulama durumu

```bash
python -m unittest discover -s tests -v
```

Paylaşım hazırlığında Python rota sadeleştirmesinin aynı doğru üzerindeki geri dönüşleri silmesi düzeltildi. Kontrol, yatay/düşey geri dönüşler, düz ilerleme ve yinelenen noktalar için mesafenin korunmasını doğrular. Bu test tüm rota yöntemlerinin geçerliliğini veya optimalitesini kanıtlamaz.

Sonraki doğrulama çalışmaları: her ürünün ziyaret edildiğinin ve her segmentin geçilebilir koridorda olduğunun kontrolü; küçük örneklerde bağımsız kesin çözücüyle karşılaştırma; C# / Python eşdeğerliği; aynı seed ile yöntem karşılaştırmaları; gerçek ölçülerle mesafe kalibrasyonu. Windows Forms ve 3D arayüz bu paylaşım hazırlığında çalıştırılarak doğrulanmamıştır.

## Yol haritası

- [ ] Python 2D ile 3D arasında depo, sipariş ve rota durumunu paylaşmak.
- [ ] Depoda gezinme deneyimini, etkileşimi ve rota takibini geliştirmek.
- [ ] Algoritma geçerlilik ve optimalite kontrollerini genişletmek.
- [ ] C# / Python sürümlerini sabit senaryolarla karşılaştırmak.
- [ ] Gerçek depo ölçüleri ve operasyon verileriyle modeli kalibre etmek.

## Geliştirme süreci

Proje ders kapsamında C# ile başladı; Python'a taşıma ve algoritma çalışmalarıyla devam ediyor. Algoritmaları anlamak, kodu geliştirmek ve sorunları incelemek için yapay zekâ araçları ve destek aldığım kişilerle yoğun biçimde çalıştım. Yapay zekâ desteği geliştirme sürecinin parçasıdır; uygulamanın rota üretmesi otomatik olarak bir makine öğrenmesi modeli kullandığı anlamına gelmez.

**Geliştiren:** İbrahim Güven — Endüstri Mühendisliği öğrencisi.
