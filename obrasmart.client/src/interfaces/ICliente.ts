export interface ICliente {
  id?: string | null;
  nombre: string;
  rut: string;
  correo: string;
  telefono: string;
  direccion: string;
  regionId?: number | null;
  ciudadId?: number | null;
}
