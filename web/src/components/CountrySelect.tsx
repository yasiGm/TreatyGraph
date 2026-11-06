import type { CountryMeta } from "../types";

interface Props {
  countries: CountryMeta[];
  value: number;
  onChange: (c: number) => void;
}

export function CountrySelect({ countries, value, onChange }: Props) {
  return (
    <div className="card picker">
      <label>
        <span>Country</span>
        <select value={value} onChange={(e) => onChange(Number(e.target.value))}>
          {countries.map((c) => (
            <option key={c.c} value={c.c}>
              {c.name}
            </option>
          ))}
        </select>
      </label>
    </div>
  );
}
