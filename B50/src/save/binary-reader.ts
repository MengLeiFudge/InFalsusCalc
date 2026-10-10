export class BinaryReader {
  private readonly view: DataView;

  constructor(private readonly bytes: Uint8Array) {
    this.view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
  }

  get length(): number {
    return this.bytes.byteLength;
  }

  hasRange(offset: number, length: number): boolean {
    return Number.isInteger(offset)
      && Number.isInteger(length)
      && offset >= 0
      && length >= 0
      && offset <= this.length - length;
  }

  readUint8(offset: number): number {
    this.assertRange(offset, 1);
    return this.view.getUint8(offset);
  }

  readUint16LE(offset: number): number {
    this.assertRange(offset, 2);
    return this.view.getUint16(offset, true);
  }

  readInt32LE(offset: number): number {
    this.assertRange(offset, 4);
    return this.view.getInt32(offset, true);
  }

  readBigUint64LE(offset: number): bigint {
    this.assertRange(offset, 8);
    return this.view.getBigUint64(offset, true);
  }

  readBytes(offset: number, length: number): Uint8Array {
    this.assertRange(offset, length);
    return this.bytes.subarray(offset, offset + length);
  }

  private assertRange(offset: number, length: number): void {
    if (!this.hasRange(offset, length)) {
      throw new RangeError(`Read outside file boundary at offset ${offset}.`);
    }
  }
}
