# C# Proje Analizi ve Python'a Tasima Plani

Bu notlar ilk inceleme icindir. Eski C# kodlari degistirilmedi. Yeni calisma `python_version/` klasorunde ayri tutuluyor.

## 1. C# dosyalari ve siniflar

| C# dosyasi | Icerik / gorev | Not |
|---|---|---|
| `Program.cs` | Uygulamanin baslangic noktasi. `LoginForm` aciliyor. | Python'da `main.py` karsiligi olacak. |
| `LoginForm.cs` | Kullanici girisi ekrani. | Ilk 3D prototipte yok, sonra basit giris eklenebilir. |
| `AdminLoginForm.cs` | Admin girisi. | Sonraki asama. |
| `AdminForm.cs` | Kullanici yonetimi, rota istatistikleri, arac ayarlari, raporlama. | Python'da ayri admin veya rapor ekrani olabilir. |
| `AuthService.cs` | Kullanici dogrulama, sifre hashleme, rol bilgisi. | `services/auth_service.py` gibi tasinabilir. |
| `SqlDatabaseService.cs` | SQLite baglantisi, tablo olusturma, ayar okuma/yazma. | `services/database.py` karsiligi. |
| `StatsService.cs` | Rota istatistigi kaydi, arac maliyeti, aylik ozetler. | `services/stats_service.py` karsiligi. |
| `AppText.cs` | Cok dilli metinler. | 3D prototip icin gerekli degil. |
| `Form1.cs` | Ana depo ekrani. Depo modeli, siparis secimi, manuel rota, optimal/Ratliff-Rosenthal, S-shape, Combined ve Combined+ mantigi buyuk oranda burada. | Python tasimanin ana kaynagi. |
| `Form1.Designer.cs` | WinForms arayuz elemanlari. | Python 3D tarafinda yeniden kurulacak, birebir tasinmayacak. |

## 2. Form1 icindeki ana modeller

| C# ic sinifi / alan | Gorev | Python karsiligi |
|---|---|---|
| `RackCell` | Bir urun lokasyonu. 2D rectangle, index, secili/toplandi bilgisi ve raf seviyesi listeleri var. | `warehouse.warehouse_model.RackCell` |
| `CrossoverPoint` | Dikey koridorlar ile yatay gecis koridorlarinin kesistigi nokta. | `warehouse.warehouse_model.CrossoverPoint` |
| `RouteNode` | Optimal rota icin depo veya urun dugumu. | `routing.route_manager.RouteNode` veya basit tuple/dataclass |
| `RouteDrawSegment` | 2D ekranda rota cizerken bolunmus segment. | 3D'de gerekli degil, ama dogrulama icin kullanilabilir. |
| `RatliffRosenthalAisle` | Her koridorda urun var mi, en ust/en alt urun nerede, largest-gap sinirlari nerede. | `routing.roodbergen.RoodbergenAisle` |
| `RatliffRosenthalCandidate` | Aday rota ve maliyet. | `routing.roodbergen.RouteCandidate` |

## 3. Depo modelinin veri akisi

1. Kullanicidan `blocks`, `aisles`, `locations`, `depot`, `shelfCount`, opsiyonel olculer ve depo yuksekligi aliniyor.
2. `BuildLayout()` calisiyor.
3. Dikey koridor X koordinatlari uretiliyor:
   `StartX + i * (CorridorW + 2 * CellW)`.
4. Yatay cross-aisle Y koordinatlari uretiliyor:
   `StartY + j * (locations * CellH + CrossAisleH)`.
5. Her X/Y kesisimine `CrossoverPoint` ekleniyor.
6. Her blok icin sol tek raf, aradaki cift raf gruplari ve sag tek raf uretiliyor.
7. Depo sinirlari ve depot rectangle hesaplanarak cizim hazir hale geliyor.

Python'da ayni mantik once 2D grid modeli olarak uretilecek. 3D sahne bu 2D modeli metre benzeri koordinata cevirerek raf bloklarini, koridorlari ve urun markerlarini olusturacak.

## 4. Siparis mantigi

1. Rastgele siparis icin kullanici urun sayisi giriyor.
2. Tum eski secimler temizleniyor.
3. `hucreler` listesinden tekrarsiz rastgele lokasyonlar seciliyor.
4. Her secilen hucreye rastgele raf seviyesi atanabiliyor.
5. Manuel sipariste kullanici 2D depoda hucreye tikliyor ve ilgili raf seviyesi secili hale geliyor.

Python'da `orders/order_model.py` siparis ve siparis kalemlerini tutacak. Prototipte 10 sabit ornek lokasyon kullandim; gercek entegrasyonda C# ile ayni rastgele seed verilerek ayni siparis uretimi de test edilebilir.

## 5. Rota algoritmasi veri akisi

### Ortak hazirlik

1. Secili urunler en yakin dikey koridora baglaniyor: `GetSelectedItemsByNearestAisle`.
2. Rota noktalari `Point(x, y)` listesi olarak olusuyor.
3. Mesafe Manhattan mesafesi ile toplanip `CellW`'ye bolunuyor.
4. Rota segmentleri islendikce secili hucreler `picked` durumuna aliniyor.

### Optimal / Roodbergen-Ratliff-Rosenthal

- Eger sadece iki cross-aisle varsa `RunRatliffRosenthalRoute` kullaniliyor.
- Her koridor icin en ust urun, en alt urun ve largest-gap sinirlari hesaplaniyor.
- Once "sadece on hattan gir-cik" adayi olusturuluyor.
- Sonra tum `upIndex/downIndex` araliklari denenerek loop adaylari uretiliyor.
- En dusuk Manhattan maliyetli aday seciliyor.
- Daha fazla cross-aisle varsa metric-closure yaklasimi ile dugumler arasi en kisa ag mesafesi hesaplanip kapali tur DP ile cozuluyor.

### S-shape / Largest Gap / Aisle-by-aisle

- C# tarafinda `button6_Click` dinamik S-shape benzeri bir yol kuruyor.
- Urun olan koridorlar soldan saga geziliyor.
- `goingUp` degiskeni ile yukari/asagi yon degistiriliyor.
- Her koridorda tam en ust/en alt yerine urunun hizasini kurtaran en yakin cross-aisle seciliyor.
- Sonunda alt cross-aisle uzerinden depoya donuluyor.

### Combined / Combined+

- `button7_Click` Combined algoritmasinin ana dinamik programlama yapisini iceriyor.
- Her urunlu koridor icin iki durum tutuluyor: koridor sonunda ustte kalmak (`costTop`) veya altta kalmak (`costBot`).
- Secenekler:
  - Ustten gelip ustte donmek.
  - Alttan gelip koridoru boydan boya gecmek.
  - Ustten gelip boydan boya alta inmek.
  - Alttan gelip alta donmek.
- Her adimda daha dusuk maliyetli onceki durum secilip path listesi buyutuluyor.
- Son durumda depo altta oldugu icin `pathBot[N-1]` tercih edilip depoya donuluyor.
- `RunCombinedRoute("Combined +")` su anda ayni `button7_Click` mantigini calistiriyor, yani Python'da ilk tasima Combined+ adini koruyup ayni sonucu uretmeli.

## 6. Python mimarisi

| Python modulu | Gorev |
|---|---|
| `main.py` | Uygulama baslatma, config/model/scene baglama. |
| `config/warehouse_config.py` | Depo olculeri ve varsayilan prototip ayarlari. |
| `warehouse/warehouse_model.py` | `RackCell`, `CrossoverPoint`, `WarehouseLayout`, `RoutePoint`. |
| `warehouse/warehouse_generator.py` | C# `BuildLayout` mantiginin Python karsiligi. |
| `orders/order_model.py` | Siparis ve siparis kalemleri. |
| `routing/combined.py` | Combined ve Combined+ DP mantigi. |
| `routing/roodbergen.py` | Ratliff-Rosenthal/Roodbergen aday rota mantigi. |
| `routing/route_manager.py` | Ornek veya gercek siparisi algoritmaya verip 3D sahneye rota hazirlama. |
| `visualization3d/warehouse_scene.py` | Ursina sahnesi, zemin, duvar, raflar, isiklar. |
| `visualization3d/player_controller.py` | FPS hareket, WASD, Shift, collision. |
| `visualization3d/camera_controller.py` | FPS/overview kamera gecisi. |
| `visualization3d/route_renderer.py` | Rota cizgisi, oklar, urun markerlari. |
| `ui/hud.py` | Mesafe, urun sayisi, tus bilgisi, FPS metni. |

## 7. C# -> Python tasima tablosu

| C# kaynak | Python hedefi | Tasima sekli |
|---|---|---|
| `Form1.BuildLayout`, `CreateRackGroup` | `warehouse/warehouse_generator.py` | Ayni grid mantigi, 3D koordinata donusumle. |
| `RackCell`, `CrossoverPoint` | `warehouse/warehouse_model.py` | Dataclass. |
| `button2_Click` siparis secimi | `orders/order_model.py`, `route_manager.py` | Seed destekli rastgele siparis. |
| `GetSelectedItemsByNearestAisle` | `routing/route_manager.py` | Algoritmalarin ortak yardimcisi. |
| `button7_Click` | `routing/combined.py` | Combined/Combined+ DP ayni kararlarla. |
| `BuildRatliffRosenthalRoute` ve yardimcilari | `routing/roodbergen.py` | Aday rota uretimi aynen. |
| `RunMetricClosureOptimalRoute`, `SolveShortestClosedTour` | `routing/roodbergen.py` veya `routing/route_manager.py` | DP kapali tur cozumleyici. |
| `CalculateRouteDistance`, `ManhattanDistance` | `routing/route_manager.py` | Piksel/cell olcegi uyumlu mesafe. |
| `DrawRacks`, `DrawRoute`, `DrawDepotBox` | `visualization3d/*` | 2D cizim yerine 3D entityler. |
| `StatsService.cs` | `services/stats_service.py` | Sonraki asamada SQLite ile. |
| `SqlDatabaseService.cs` | `services/database.py` | Sonraki asamada. |

## 8. Ayni sonucun dogrulanmasi

1. C# tarafinda belirli depo parametreleri secilir: ornegin `blocks=2`, `aisles=5`, `locations=5`, `shelfCount=1`.
2. Rastgelelik yerine sabit urun index listesi kullanilir. Gerekirse C#'tan rota noktalarini kopyalayacak kucuk bir debug cikisi eklenir ama ana kaynak degistirilmez.
3. Python generator ayni `RackCell.Index`, `Bounds.X`, `Bounds.Y` degerlerini uretmeli.
4. Ayni secili hucreler Python'da order olarak isaretlenir.
5. Combined+, Roodbergen ve Optimal icin Python rota noktalari C# rota noktalariyla karsilastirilir.
6. Mesafe karsilastirmasi C# formuluyle yapilir: toplam Manhattan piksel mesafesi / `CellW`, 1 ondaliga yuvarlama.
7. Kabul kriteri:
   - Ayni algoritmada rota noktasi listesi ayniysa dogrudan basarili.
   - Bazi esdeger yollar farkli sirada olursa toplam mesafe 0.1 hucre fark icinde kalmali.
8. Daha sonra 3D tarafinda ayni rota noktalarinin dunya koordinatina dogru cevrildigi kontrol edilir. Rota mesafesi algoritmadan gelir; 3D cizgi sadece gorsellestirmedir.

