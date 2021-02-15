import { NetworkInterfaceInfo, networkInterfaces } from "os";

const PUBLIC_IP_TYPE: string = "IPv4";
const PRIVATE_IP_PREFIX: string = "192.168";

/**
 * Checks if ip is valid
 */
export const isValidIp = (ip: string) => {
    // Regex reference : http://ipregex.com/
    // tslint:disable-next-line:max-line-length
    return ip.match(/^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/);
};

/**
 * Returns the first external IPv4 IP found on the machine.
 *
 * @remarks
 * Reference used : https://stackoverflow.com/a/8440736
 */
export const getPublicIp = () => {
    const netInterfaces: { [index: string]: NetworkInterfaceInfo[] } = networkInterfaces();
    let address;
    Object.keys(netInterfaces)
    .forEach((interfaceName: string) => {
        for (const interfaceInfo of netInterfaces[interfaceName]) {
            if (isPublicIpv4(interfaceInfo)) {
                address = interfaceInfo.address;
              }
        }
    });

    return address;
};

/**
 * Checks if the interfaceInfo is tied to a public IPv4 IP.
 */
const isPublicIpv4 = (interfaceInfo: NetworkInterfaceInfo) => {
    return PUBLIC_IP_TYPE === interfaceInfo.family
        && interfaceInfo.internal === false
        && !interfaceInfo.address.startsWith(PRIVATE_IP_PREFIX);
};
