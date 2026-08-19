export interface IRecursoItemPresupuesto {
  id?: string;
  tipoInsumo: string;
  descripcionCongelada: string;
  cantidad: number;
  precioUnitarioCongelado: number;
  costoTotalRecurso?: number; // Calculado por el backend
  unidadMedidaId: number | null;
  unidadMedidaNombre?: string;
}

export interface IItemPresupuesto {
  id?: string;
  estructuraAPUOrigenId?: string | null;
  descripcion: string;
  cantidadItem: number;
  precioUnitarioCalculado?: number; // Calculado por el backend
  subtotal?: number; // Calculado por el backend
  unidadMedidaId: number | null;
  unidadMedidaNombre?: string;
  recursos: IRecursoItemPresupuesto[];
}

export interface IPresupuesto {
  id?: string;
  clienteId?: string | null;
  clienteNombre?: string; // Solo lectura
  clienteRut?: string; // Solo lectura
  clienteDireccion?: string; // Solo lectura
  nombreProyecto: string;
  fechaCreacion?: string; // Solo lectura
  estado?: string; // Solo lectura
  subtotal?: number; // Calculado por el backend
  montoIva?: number; // Calculado por el backend
  total?: number; // Calculado por el backend
  items: IItemPresupuesto[];
  esPlantilla: boolean;
}
