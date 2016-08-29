/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

export default ['User', ['$q', '$injector', '$rootScope', '$state', '$http', function($q, $injector, $root, $state, $http) {
    let user = null;

    return {
        load() {
            return $http.post('/api/v1/user')
                .then(({data}) => {
                    user = $root.user = data;

                    $root.$broadcast('user', user);

                    return user;
                })
                .catch(({data}) => $q.reject(data));
        },
        read() {
            if (user)
                return $q.resolve(user);

            return this.load();
        },
        clean() {
            delete $root.user;

            delete $root.IgniteDemoMode;

            sessionStorage.removeItem('IgniteDemoMode');
        }
    };
}]];