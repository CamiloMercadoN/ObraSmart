export interface IComponenteAPU {
  insumoId: string ;
  descripcionInsumo: string;
  tipoInsumo: string;
  precioUnitarioReferencia: number;
  cantidad: number;
  subtotal: number;
}

export interface IEstructuraAPU {
  id: string;
  nombre: string;
  unidadMedidaId: number;
  unidadMedidaNombre: string;
  costoTotalCalculado: number;
  esPlantilla: boolean;
  etiquetasIds: string[];
  componentes: IComponenteAPU[];
}

export interface IComponenteAPUInput {
  insumoId: string;
  cantidad: number;
}

export interface IEstructuraAPUUpsert {
  nombre: string;
  unidadMedidaId: number;
  etiquetasIds: string[];
  componentes: IComponenteAPUInput[];
}
