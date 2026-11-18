function Outline({ viewBox, strokeWidth = 2.2, children }) {
  return (
    <svg
      className="vehicle-art"
      viewBox={viewBox}
      fill="none"
      stroke="currentColor"
      strokeWidth={strokeWidth}
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      {children}
    </svg>
  );
}

function ForkliftArt() {
  return (
    <Outline viewBox="0 0 72 52">
      <path d="M14 8h26v26H14z" />
      <path d="M40 14h12l10 12v10H40z" />
      <path d="M52 4v40" />
      <path d="M52 44h18" />
      <circle cx="24" cy="40" r="6" />
      <circle cx="48" cy="40" r="6" />
      <path d="M18 14h18" />
    </Outline>
  );
}

function CarArt() {
  return (
    <Outline viewBox="0 0 84 34">
      <path d="M6 22h12l10-9h26l12 9h12v7H6z" />
      <path d="M28 13h22l8 9H20z" />
      <circle cx="22" cy="29" r="5" />
      <circle cx="64" cy="29" r="5" />
    </Outline>
  );
}

function PickupArt() {
  return (
    <Outline viewBox="0 0 92 38">
      <path d="M6 24h12l9-11h26l11 11h22v7H6z" />
      <path d="M29 13h20l8 11H21z" />
      <circle cx="24" cy="31" r="5" />
      <circle cx="72" cy="31" r="5" />
    </Outline>
  );
}

function VanArt() {
  return (
    <Outline viewBox="0 0 88 43">
      <path d="M8 37V14l14-8h40l18 12v19z" />
      <path d="M24 10h28l12 8H24z" />
      <circle cx="26" cy="37" r="6" />
      <circle cx="68" cy="37" r="6" />
    </Outline>
  );
}

function CargoTruckArt() {
  return (
    <Outline viewBox="0 0 110 46">
      <path d="M6 12h64v26H6z" />
      <path d="M76 20h18l10 8v10H76z" />
      <path d="M70 32h6" />
      <circle cx="24" cy="38" r="6" />
      <circle cx="86" cy="38" r="6" />
      <circle cx="98" cy="38" r="6" />
    </Outline>
  );
}

function TractorArt() {
  return (
    <Outline viewBox="0 0 188 50">
      <path d="M10 32h116V18h24l20 12v8H10z" />
      <path d="M132 20h18l14 10h-32z" />
      <circle cx="38" cy="40" r="7" />
      <circle cx="122" cy="40" r="7" />
      <circle cx="158" cy="40" r="7" />
    </Outline>
  );
}

function BusArt() {
  return (
    <Outline viewBox="0 0 132 47">
      <path d="M6 10h108l12 12v18H6z" />
      <path d="M14 15h90l8 8H14z" />
      <circle cx="30" cy="40" r="6" />
      <circle cx="108" cy="40" r="6" />
    </Outline>
  );
}

function MotorcycleArt() {
  return (
    <Outline viewBox="0 0 54 34">
      <circle cx="12" cy="25" r="8" />
      <circle cx="42" cy="25" r="8" />
      <path d="M12 25l10-10h10l10 10" />
      <path d="M22 15h10" />
      <path d="M42 17h8" />
    </Outline>
  );
}

function CraneArt() {
  return (
    <Outline viewBox="0 0 96 44">
      <path d="M8 30h52V16H8z" />
      <path d="M66 18h14l10 10v4H66z" />
      <path d="M30 16L64 6h18" />
      <path d="M82 6v14" />
      <path d="M82 20a4 4 0 1 1-8 0" />
      <circle cx="24" cy="34" r="6" />
      <circle cx="78" cy="34" r="6" />
    </Outline>
  );
}

export const FLEET_VEHICLES = [
  { id: "montacargas", label: "Montacargas", w: 72, h: 52, Art: ForkliftArt },
  { id: "automovil", label: "Automóvil", w: 84, h: 34, Art: CarArt },
  { id: "camioneta", label: "Camioneta", w: 92, h: 38, Art: PickupArt },
  { id: "furgoneta", label: "Furgoneta", w: 88, h: 43, Art: VanArt },
  { id: "camion-carga", label: "Camión de carga", w: 110, h: 46, Art: CargoTruckArt },
  { id: "cabezal", label: "Cabezal", w: 188, h: 50, Art: TractorArt },
  { id: "bus", label: "Bus", w: 132, h: 47, Art: BusArt },
  { id: "motocicleta", label: "Motocicleta", w: 54, h: 34, Art: MotorcycleArt },
  { id: "grua", label: "Grúa", w: 96, h: 44, Art: CraneArt },
];
