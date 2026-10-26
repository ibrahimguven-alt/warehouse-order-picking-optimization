# Ibonun Deposu - Python Rota Optimizasyonu

Bu klasor C# projesinden ayri tutuldu. Ana uygulama 2D rota optimizasyon ekranidir. 3D depo gezme modu sadece ek ozelliktir.

Calistirma:

```powershell
cd python_version
python -m pip install -r requirements.txt
python main.py
```

Ana ekranda:

- Depo blok/koridor/lokasyon sayisini yaz.
- `Depoyu olustur` ile 2D depoyu ciz.
- `Rastgele siparis olustur` veya rafa tiklayarak urun sec.
- `S-Shape`, `Combined +` veya `Roodbergen` ile rota hesapla.
- `3D depoda gez` ile ayrica 3D moda gec.

3D kontroller:

- `W A S D`: yurume
- `Mouse`: etrafa bakma
- `Shift`: hizli yurume
- `F`: FPS / tepeden bakis
- `R`: rota cizgisini ac/kapat
- `M`: urun markerlarini ac/kapat
- `T`: baslangic noktasina don
- `Esc`: mouse kilidini ac/kapat

Not: Bu surum ogrenci projesi gibi sade tutuldu; rota mantigi okunabilir modullere ayrildi.
