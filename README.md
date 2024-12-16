# ToTraveler
Vietovių pildymo ir jų vertinimo sistema.

Saitynas pasiekiamas per http://20.215.233.177/.

## Funkciniai reikalavimai
Use case diagrama:
![Alt text](./readme/usecase.png)

## Sistemos architektūra 
UML deployment diagrama:
![Alt text](./readme/deployment.png)

## Naudotojo sąsajos projektas
Kadangi projektas nedidelis, langų wireframe'ai nebuvo naudojami. Svetainę realizavau be wireframe'ų, reverse engineering (website to wireframe) nedariau. Esu naudojęs Figma su Photoshop frontend dizaino kūrimui. Suprantu jog susidėliot dideliai ir sudėtingai svetainei visą dizainą  su atitinkamais įrankiais yra dažnai paprasčiau ir pigiau negu jį suprogramuot.

### Pagrindinis puslapis
![Pagrindinis puslapis](homepage.png)

### Registracijos forma
![Registracijos forma](register.png)

### Prisijungimo forma
![Prisijungimo forma](login.png)

### Svečiui matomos lokacijos
![Svečiui matomos lokacijos](guest_locations.png)

### Tik administratoriui prieinamas kategorijų valdymo puslapis
![Kategorių valdymas](categories.png)

### Lokacijų pridėjimo forma
Galima formą lengvai papildyti naudojant Google Maps nuorodą.
![Lokacijų pridėjimo forma](location_add.png)

### Lokacijos redagavimo forma
![Lokacijos redagavimo forma](location_edit.png)


## API specifikacija
API specifikacija pateikta [api-spec.yaml](/api-spec.yaml) su galimais response kodais, bet užklausų schemų pavyzdžiais.

## Išvados
- Projekto pradinė apimtis buvo per didelė modulio atžvilgiu, teko susiprastinti.
- Prisiminiau ir pagilinau RESTful API principus.
- Išmokau hostint sveitainę.
- Pirmas realizuotas projektas naudojat ReactJs kartu su Vite.

