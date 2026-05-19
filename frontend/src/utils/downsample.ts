/**
 * Downsamples an array of data points by bucketing and averaging values.
 * Keeps the chart readable when there are too many points.
 */
export function downsample<T extends { value: number }>(
  data: T[],
  maxPoints: number,
): T[] {
  if (data.length <= maxPoints) return data;

  const bucketSize = data.length / maxPoints;
  const result: T[] = [];

  for (let i = 0; i < maxPoints; i++) {
    const start = Math.floor(i * bucketSize);
    const end = Math.floor((i + 1) * bucketSize);
    const bucket = data.slice(start, end);

    const avgValue =
      bucket.reduce((sum, p) => sum + p.value, 0) / bucket.length;
    // Use the middle element as the representative point (preserves its label)
    const mid = Math.floor(bucket.length / 2);
    result.push({ ...bucket[mid], value: +avgValue.toFixed(1) });
  }

  return result;
}
