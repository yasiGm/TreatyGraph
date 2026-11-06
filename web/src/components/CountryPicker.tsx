import type { CountryMeta } from "../types";

interface Props {
  countries: CountryMeta[];
  a: number;
  b: number;
  onChange: (a: number, b: number) => void;
}

export function CountryPicker({ countries, a, b, onChange }: Props) {
  const options = countries.map((c) => (
    <option key={c.c} value={c.c}>
      {c.name}
    </option>
  ));

  return (
    <div className="card picker">
      <label>
        <span>Country 1</span>
        <select value={a} onChange={(e) => onChange(Number(e.target.value), b)}>
          {options}
        </select>
      </label>
      <button className="swap" type="button" aria-label="Swap countries" onClick={() => onChange(b, a)}>
        ⇄
      </button>
      <label>
        <span>Country 2</span>
        <select value={b} onChange={(e) => onChange(a, Number(e.target.value))}>
          {options}
        </select>
      </label>
    </div>
  );
}
