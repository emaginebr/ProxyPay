function calcDigit(digits: number[]): number {
  const sum = digits.reduce(
    (acc, d, i) => acc + d * (digits.length + 1 - i),
    0
  );
  const rest = (sum * 10) % 11;
  return rest === 10 ? 0 : rest;
}

export function generateRandomCpf(): string {
  const base = Array.from({ length: 9 }, () => Math.floor(Math.random() * 10));
  const d1 = calcDigit(base);
  const d2 = calcDigit([...base, d1]);
  return [...base, d1, d2].join("");
}
