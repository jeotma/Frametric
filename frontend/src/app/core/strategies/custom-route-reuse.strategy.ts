import { RouteReuseStrategy, ActivatedRouteSnapshot, DetachedRouteHandle } from '@angular/router';

export class CustomRouteReuseStrategy implements RouteReuseStrategy {
  private handlers: { [key: string]: DetachedRouteHandle } = {};

  public shouldDetach(route: ActivatedRouteSnapshot): boolean {
    // Cache all route states to preserve user inputs, active tabs, and results when navigating away
    return !!route.routeConfig;
  }

  public store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle | null): void {
    if (route.routeConfig) {
      const key = this.getRouteKey(route);
      if (handle) {
        this.handlers[key] = handle;
      } else {
        delete this.handlers[key];
      }
    }
  }

  public shouldAttach(route: ActivatedRouteSnapshot): boolean {
    if (!route.routeConfig) return false;
    const key = this.getRouteKey(route);
    return !!this.handlers[key];
  }

  public retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {
    if (!route.routeConfig) return null;
    const key = this.getRouteKey(route);
    return this.handlers[key] || null;
  }

  public shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {
    return future.routeConfig === curr.routeConfig;
  }

  private getRouteKey(route: ActivatedRouteSnapshot): string {
    // Generate a unique cache key incorporating path and route params (e.g. movie id)
    let path = route.routeConfig?.path || '';
    let current = route;
    while (current.parent) {
      current = current.parent;
      if (current.routeConfig?.path) {
        path = current.routeConfig.path + '/' + path;
      }
    }
    return `${path}?${JSON.stringify(route.params)}`;
  }
}
