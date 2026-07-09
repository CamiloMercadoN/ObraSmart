export interface ICliente {
  id?: string | null;
  nombre: string;
  rut: string;
  correo: string;
  telefono: string;
  direccion: string;
  RegionId?: number | null;
  ciudadId?: number | null;
}
