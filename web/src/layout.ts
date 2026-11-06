export interface Span {
  start: number;
  end: number;
}

// Give every span a row number so that overlapping spans do not hide each other.
export function packRows(spans: Span[]): { rows: number[]; count: number } {
  const order = spans.map((_, i) => i).sort((i, j) => spans[i].start - spans[j].start);
  const rowEnds: number[] = [];
  const rows: number[] = new Array<number>(spans.length).fill(0);

  for (const i of order) {
    let row = rowEnds.findIndex((end) => end <= spans[i].start);
    if (row < 0) {
      row = rowEnds.length;
      rowEnds.push(0);
    }
    rowEnds[row] = spans[i].end;
    rows[i] = row;
  }
  return { rows, count: Math.max(1, rowEnds.length) };
}
